using AntMe_2_Lib.Definitions;
using AntMe_2_Lib.Helper.ExtensionMethods;
using AntMe_2_Lib.Simulator;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;

public partial class GameManager : Node
{

	RandomNumberGenerator rand = new RandomNumberGenerator();

    public enum GameStateEnum { NONE, RUNNING, PAUSE, END };

	public static GameStateEnum GameState = GameStateEnum.NONE;
	List<AntSimulator> AntSimulatorList = new List<AntSimulator>();

	public List<BaseMaterial3D> Colors = new List<BaseMaterial3D>()
	{
		ResourceLoader.Load("res://Materials/mat_color_red.tres") as BaseMaterial3D,
        ResourceLoader.Load("res://Materials/mat_color_yellow.tres") as BaseMaterial3D,
        ResourceLoader.Load("res://Materials/mat_color_blue.tres") as BaseMaterial3D,
        ResourceLoader.Load("res://Materials/mat_color_black.tres") as BaseMaterial3D
    };

	TimeSpan TimeSpanSecond = new TimeSpan(0, 0, 0, 1, 0);
	DateTime LastUpdate = DateTime.MinValue;
	int RoundCounter = 1;

    //List<Ant> Ants = new List<Ant>();
    System.Collections.Generic.Dictionary<Guid, BaseMaterial3D> PlayerColor = new System.Collections.Generic.Dictionary<Guid, BaseMaterial3D>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PrintDebug("Starte Game");
 
		ColonyManager.LoadingUserAntsByDll();
 
		PrintDebug("Alles geladen");

		StartSimulation();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GameState == GameStateEnum.RUNNING
			&& (LastUpdate + (TimeSpanSecond / GameSettings.RoundSpeed) <= DateTime.Now))
		{
			RoundCounter++;
			PrintDebug($"GAME {GameState} ROUND {RoundCounter}");
			LastUpdate = DateTime.Now;

			foreach(PlayerColony colony in ColonyManager.Colonys.Values)
			{
				BornAnt(colony);
				colony.DoTicks();
			}

			if (RoundCounter >= GameSettings.Rounds)
				GameState = GameStateEnum.END;

			UpdateAnts();
		}

		if(GameState == GameStateEnum.END)
		{
			GetTree().Quit();
		}
			
	}



    void UpdateAnts()
    {

    }


    public override void _UnhandledInput(InputEvent @event)
    {
        Camera3D camera = GetParent().GetNode<Camera3D>("Camera3D");
        Vector3 cameraPos = camera.Position;

        base._UnhandledInput(@event);
		if(@event is InputEventMouseMotion)
		{
			InputEventMouseMotion inputEventMouseMotion = (InputEventMouseMotion) @event;

            camera.Position +=  (new Vector3(inputEventMouseMotion.Relative.X, 0, inputEventMouseMotion.Relative.Y) * 0.1f);
        }

		if(@event is InputEventMouseButton)
		{
			InputEventMouseButton inputEventMouseButton = (InputEventMouseButton) @event;
			if(inputEventMouseButton.IsPressed())
			{
				switch(inputEventMouseButton.ButtonIndex)
				{
					case MouseButton.WheelUp:
						camera.Position += new Vector3(0, -1, -1);
                        break;

                    case MouseButton.WheelDown:
                        camera.Position += new Vector3(0, 1, 1);
						break;

                }

            }
		}


        if (@event is InputEventKey eventKey)
            if (eventKey.Pressed && eventKey.Keycode == Key.Escape)
                GetTree().Quit();
    }


    void BornAnt(PlayerColony colony)
	{
		AntSimulator sim = ColonyManager.AntBorn(colony, this);
		if (sim == null)
			return;

        sim.ShowSimDebugInfoEvent += AntSimulator_ShowDebugInfoEvent;

        PackedScene ps = ResourceLoader.Load("res://Szenes/Models/ant.tscn") as PackedScene;
        Ant ant = ps.Instantiate() as Ant;

		ant.SIM = sim;

		//Material
		MeshInstance3D mesh = ant.GetNode("MeshInstance3D") as MeshInstance3D;
		mesh.MaterialOverlay = PlayerColor[colony.Guid];

		Node parent = GetParent();
		parent.AddChild(ant);

		ant.Position = (colony.StartPosition + new System.Numerics.Vector3(0, 2, 0)).ToGodot();
		ant.RotateY(rand.RandfRange(0, 360));

        ant.SIM.Init();
    }


	void StartSimulation()
	{
		PrintDebug("Starte Simulation");

		int colorIndex = 0;

        rand.Seed = GameSettings.GameSeed > 0 ? GameSettings.GameSeed : (ulong)DateTime.Now.Ticks;

		//Camera positions
		float posX = 0;
		float posY = 0;
		float posZ = 0;

        foreach (PlayerColony colony in ColonyManager.Colonys.Values)
        {
			//Choosing Colors
			PlayerColor.Add(colony.Guid, Colors[colorIndex]);
			colorIndex++;

			//Coosing StartPosition
			//TODO: Range of instance position of spawning
			colony.StartPosition = new System.Numerics.Vector3(rand.RandfRange(-40, 40), 0, rand.RandfRange(-40, 40));

			posX += colony.StartPosition.X;
			posY += colony.StartPosition.Y;
			posZ += colony.StartPosition.Z;
        }

        //camera position
        posX /= ColonyManager.Colonys.Count;
		posY /= ColonyManager.Colonys.Count;
		posZ /= ColonyManager.Colonys.Count;

		posY += 10;
		posZ += 20;

		Camera3D camera = GetParent().GetNode<Camera3D>("Camera3D");
        camera.Translate(new Vector3(posX, posY, posZ));


        GameState = GameStateEnum.RUNNING;
	}


	public void AntSimulator_ShowDebugInfoEvent(object sender, string e)
	{
		PrintDebug(sender, e);
	}


	public static void PrintDebug(string text)
	{
		GD.Print(text);
	}

	public static void PrintDebug(object sender, string text)
	{
		AntSimulator sim = (AntSimulator)sender;
		string name = string.Format("{0}/{1} ({2}) ", sim.SimPlayer.ColonyName, sim.SimCaste.Name, sim.Guid.ToString());

        GD.Print(text != null ? string.Format("{0} => {1}", sender.ToString(), text) : sender.ToString());
	}
}
