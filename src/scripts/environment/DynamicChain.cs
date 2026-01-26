using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class DynamicChain : Node3D
{
    [ExportCategory("References")]
    [Export] public StaticBody3D Anchor { get; set; }
    [Export] public Node3D LinkContainer { get; set; }
    
    [ExportCategory("Chain Settings")]
    [Export] public int LinkCount
    {
        get => _linkCount;
        set => _linkCount = Mathf.Clamp(value, 2, 50);
    }
    [Export] public float LinkLength { get; set; } = 0.3f;
    [Export] public float LinkRadius { get; set; } = 0.05f;

    private int _linkCount = 10;
    private List<RigidBody3D> Links { get; set; } = [];
    private List<Generic6DofJoint3D> Joints { get; set; } = [];

    public override void _Ready()
    {
        base._Ready();
        if (!Engine.IsEditorHint())
        {
            GetTree().ProcessFrame += CreateJoints;
            GenerateChain();
        }
    }

    public void GenerateChain()
    {
        //clear any existing chain
        foreach(var link in Links)
        {
            if (IsInstanceIdValid(link.GetInstanceId()))
            {
                link.QueueFree();
            }
        }
        Links.Clear();
        Joints.Clear();

        foreach (var child in LinkContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        //generate links
        for (int i = 0; i < LinkCount; i++)
        {
            var link = CreateLink(i);
            LinkContainer.AddChild(link);
            Links.Add(link);
            link.Position = new Vector3(0, -(i + 1) * LinkLength, 0);
        }
    }

    public void CreateJoints()
    {
        for (int i = 0; i < LinkCount; i++)
        {
            Node3D body1 = i == 0 ? Anchor : Links[i - 1];
            var body2 = Links[i];

            var joint = CreateJoint(body1, body2);
            body2.AddChild(joint);
            Joints.Add(joint);
        }

        GetTree().ProcessFrame -= CreateJoints;
        //tutorial at https://youtu.be/2RyDTkGrRdY?si=gIa2MZdmorfMTgjI&t=625
    }

    public RigidBody3D CreateLink(int index)
    {
        var link = new RigidBody3D();
        link.Name = $"Link_{index}";
        
        //physics properties
        link.Mass = 0.5f;
        link.GravityScale = 1.0f;
        link.LinearDamp = 0.5f;
        link.AngularDamp = 0.5f;
        
        //visual mesh
        var meshInstance = new MeshInstance3D();
        var cylinder = new CylinderMesh();
        cylinder.Height = LinkLength;
        cylinder.TopRadius = LinkRadius;
        cylinder.BottomRadius = LinkRadius;
        meshInstance.Mesh = cylinder;
        link.AddChild(meshInstance);
        
        //collision shape
        var collisionShape = new CollisionShape3D();
        var cylinderShape = new CylinderShape3D();
        cylinderShape.Height = LinkLength;
        cylinderShape.Radius = LinkRadius;
        collisionShape.Shape = cylinderShape;
        link.AddChild(collisionShape);
        
        return link;
    }

    public Generic6DofJoint3D CreateJoint(Node3D body1, RigidBody3D body2)
    {
        var joint = new Generic6DofJoint3D();
        joint.Name = $"Joint_to_{body1.Name}";
        joint.Position = new Vector3(0, LinkLength * 0.5f, 0);

        //lock x axis (no left/right stretch)
        joint.SetFlagX(Generic6DofJoint3D.Flag.EnableLinearLimit, true);
        joint.SetParamX(Generic6DofJoint3D.Param.LinearLowerLimit, 0);
        joint.SetParamX(Generic6DofJoint3D.Param.LinearUpperLimit, 0);
        
        //lock y axis (no up/down stretch)
        joint.SetFlagY(Generic6DofJoint3D.Flag.EnableLinearLimit, true);
        joint.SetParamY(Generic6DofJoint3D.Param.LinearLowerLimit, 0);
        joint.SetParamY(Generic6DofJoint3D.Param.LinearUpperLimit, 0);
        
        //lock z axis (no forward/back stretch)
        joint.SetFlagZ(Generic6DofJoint3D.Flag.EnableLinearLimit, true);
        joint.SetParamZ(Generic6DofJoint3D.Param.LinearLowerLimit, 0);
        joint.SetParamZ(Generic6DofJoint3D.Param.LinearUpperLimit, 0);
        
        //angular limits (swing range)
        var angularLimitRad = Mathf.DegToRad(30.0f);
        var twistLimitRad = Mathf.DegToRad(15.0f);
        
        //x axis swing (pitch)
        joint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
        joint.SetParamX(Generic6DofJoint3D.Param.AngularLowerLimit, -angularLimitRad);
        joint.SetParamX(Generic6DofJoint3D.Param.AngularUpperLimit, angularLimitRad);
        
        //y axis twist
        joint.SetFlagY(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
        joint.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, -twistLimitRad);
        joint.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, twistLimitRad);
        
        //z axis swing (roll)
        joint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
        joint.SetParamZ(Generic6DofJoint3D.Param.AngularLowerLimit, -angularLimitRad);
        joint.SetParamZ(Generic6DofJoint3D.Param.AngularUpperLimit, angularLimitRad);

        joint.Ready += () =>
        {
            joint.NodeA = joint.GetPathTo(body1);
            joint.NodeB = new NodePath("..");
        };
        
        return joint;
    }
}
