using AntMe_2_Lib.Simulator;
using Godot;
using System;

public partial class Ant: RigidBody3D
{
	public AntSimulator SIM;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("ERSCHIENEN");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
    }



	public void MoveForward(double delta)
	{
        Vector3 v3 = Vector3.Back;
        v3 = v3.Rotated(Vector3.Up, Rotation.Y);

        ApplyForce(v3 * (float)delta * 1000);
    }
}
