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
    
    [ExportCategory("Physics Properties")]
    [Export] public float LinkMass { get; set; } = 0.3f;
    [Export] public float GravityScale { get; set; } = 0.05f;
    [Export] public float LinkDamping { get; set; } = 0.3f;
    
    [ExportCategory("Joint Settings")]
    [Export] public float AngularLimitDegrees { get; set; } = 0.3f;
    [Export] public float TwistLimitDegrees { get; set; } = 0.05f;

    [ExportCategory("Collision Settings")]
    [Export(PropertyHint.Layers3DPhysics)]
    public uint LinkCollisionLayer { get; set; } = 1;
    [Export(PropertyHint.Layers3DPhysics)]
    public uint LinkCollisionMask { get; set; } = 1;

    [ExportGroup("Mesh Settings")]
    [Export] public ChainTypeEnum ChainType { get; set; } = ChainTypeEnum.Rope;
    [Export] public Mesh LinkMesh { get; set; } = null;
    [Export] public float MeshScale { get; set; } = 1f;
    
    [ExportGroup("Attachment")]
    [Export] public PackedScene AttachedScene { get; set; }

    public enum ChainTypeEnum
    {
        Chain,
        Rope,
    } 

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
        
        if (AttachedScene is not null && Links.Count > 0)
        {
            var attachment = (Node3D) AttachedScene.Instantiate();
            LinkContainer.AddChild(attachment);

            var bottomLink = Links[^1];
            attachment.GlobalPosition = bottomLink.GlobalPosition + new Vector3(0, -LinkLength, 0);

            if (attachment is RigidBody3D rBody)
            {
                var attachmentJoint = CreateJoint(bottomLink, rBody);
                attachment.AddChild(attachmentJoint);
            }
        }

        GetTree().ProcessFrame -= CreateJoints;
    }

    public RigidBody3D CreateLink(int index)
    {
        var link = new RigidBody3D();
        link.Name = $"Link_{index}";
        
        //physics properties
        link.Mass = LinkMass;
        link.GravityScale = GravityScale;
        link.LinearDamp = LinkDamping;
        link.AngularDamp = LinkDamping;
        link.CollisionMask = LinkCollisionMask;
        link.CollisionLayer = LinkCollisionLayer;
        
        //visual mesh        
        var meshInstance = new MeshInstance3D();

        if (ChainType is ChainTypeEnum.Chain && LinkMesh is not null)
        {
            //use custom mesh
            meshInstance.Mesh = LinkMesh;
        }
        else
        {
            //procedural
            var cylinder = new CylinderMesh();
            cylinder.Height = LinkLength;
            cylinder.TopRadius = LinkRadius;
            cylinder.BottomRadius = LinkRadius;
            meshInstance.Mesh = cylinder;
        }

        meshInstance.Scale = Vector3.One * MeshScale;
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
        var angularLimitRad = Mathf.DegToRad(AngularLimitDegrees);
        var twistLimitRad = Mathf.DegToRad(TwistLimitDegrees);
        
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
