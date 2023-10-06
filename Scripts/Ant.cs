using AntMe_2_Lib.Definitions;
using AntMe_2_Lib.Simulator;
using Godot;
using System;
using static AntMe_2_Lib.GameObject.InsectCore;

public partial class Ant: RigidBody3D
{
	public AntSimulator SIM;
    double cooldown = -1;

    CollisionShape3D ViewArea;
    MeshInstance3D SignalRing;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		GD.Print("ERSCHIENEN");
    }

    PlayerColony getColony()
    {
        return SIM.PlayerColony;
    }


    Material getPlayerColor()
    {
        PlayerColony colony = getColony();

        return GameManager.Instance.PlayerColor[colony.Guid];
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        ViewArea = GetNode<CollisionShape3D>("AreaView/CollisionView") as CollisionShape3D;
        SignalRing = GetNode<MeshInstance3D>("AreaView/MeshSignalRing") as MeshInstance3D;

        MeshInstance3D mesh = GetNode<MeshInstance3D>("Mesh") as MeshInstance3D;
        SignalRing.MaterialOverride = getPlayerColor();

        SignalRing.Scale = new Vector3(SIM.ViewRange, 1, SIM.ViewRange);
        ViewArea.Scale = new Vector3(SIM.ViewRange, 1, SIM.ViewRange);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if(cooldown > 0)
        {
            cooldown -= delta;
            if (cooldown < 0)
                hide();
        }
    }

    public void _on_area_visible_body_3_entered(Node3D body)
    {
        SIM.FindsSugar();

        SignalRing.Visible = true;
        GameManager gm = GameManager.Instance;
        MeshInstance3D sugar = body.GetParent() as MeshInstance3D;
        gm.sugars[sugar] -= SIM.Load;
        gm.updateSugar();

        cooldown = 0.5f;
    }

    public void hide()
    {
        SignalRing.Visible = false;
    }

    /// <summary>
    /// Entered a wall and goes back
    /// </summary>
    /// <param name="body"></param>
    public void _on_area_3d_body_entered(Node3D body)
    {
        Node3D body3d = (Node3D)body.GetParent();

        float gradA = RotationDegrees.Y;
        float gradW = body3d.RotationDegrees.Y;
        float c = 0f;
        float ausgang = 0f;

        if (gradA >= 0)
        {
            c = 90 - gradW - gradA;
            ausgang = (180 + gradA + (2 * c)) % 360;
        }
        else
        {
            c = 90 - gradW + gradA;
            ausgang = (-180 + gradA - (2 * c)) % 360;
        }

        RotationDegrees = new Vector3(0, ausgang, 0);
    }

    internal void Update(double delta)
    {
        InsectStateEnum state = (InsectStateEnum)SIM.AntObject.State;

        //InsectStateEnum b = (InsectStateEnum)state;

        switch (state)
        {
            case InsectStateEnum.WAITING:
                SIM.Waiting();
                break;

            case InsectStateEnum.MOVING:
                MoveForward(delta);
                break;
        }
    }

    public void MoveForward(double delta)
	{        
        Vector3 v3 = Vector3.Back;
        v3 = v3.Rotated(Vector3.Up, Rotation.Y);

        ApplyForce(v3 * (float)delta * SIM.Speed);
    }
}
