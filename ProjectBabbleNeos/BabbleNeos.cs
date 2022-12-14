using BaseX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;

namespace ProjectBabbleNeos
{
    public class BabbleNeos : NeosMod
	{
		public override string Name => "ProjectBabble-Neos";
		public override string Author => "dfgHiatus";
		public override string Version => "1.0.1";
		public override string Link => "https://github.com/dfgHiatus/Neos-Eye-Face-API/";

		private static BabbleOSC _bosc;
		private static ModConfiguration _config;

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> vrOnly = 
			new ModConfigurationKey<bool>("vrOnly", "Only run face tracking in VR Mode", () => true);

		public override void OnEngineInit()
		{
			new Harmony("net.dfgHiatus.ProjectBabble-Neos").PatchAll();
			_config = GetConfiguration();

			_bosc = new BabbleOSC();
			Engine.Current.OnReady += () =>
				Engine.Current.InputInterface.RegisterInputDriver(new ProjectBabbleDevice());
			Engine.Current.OnShutdown += () => _bosc.Teardown();
        }

		public class ProjectBabbleDevice : IInputDriver
		{
			public int UpdateOrder => 100;
			public Mouth _mouth;

			public void CollectDeviceInfos(DataTreeList list)
			{
				var mouthDataTreeDictionary = new DataTreeDictionary();
				mouthDataTreeDictionary.Add("Name", "Project Babble Face Tracking");
				mouthDataTreeDictionary.Add("Type", "Lip Tracking");
				mouthDataTreeDictionary.Add("Model", "Project Babble Model");
				list.Add(mouthDataTreeDictionary);
			}

			public void RegisterInputs(InputInterface inputInterface)
			{
				_mouth = new Mouth(inputInterface, "Project Babble Mouth Tracking");
			}

			public void UpdateInputs(float deltaTime)
			{
				_mouth.IsTracking = _config.GetValue(vrOnly);

				// Assuming x is left/right, y is up/down, z is forward/backwards
				_mouth.Jaw = new float3(
					BabbleOSC.MouthShapesWithAddress["/jawLeft"] - BabbleOSC.MouthShapesWithAddress["/jawRight"],
					BabbleOSC.MouthShapesWithAddress["/jawOpen"], // + BabbleOSC.MouthShapesWithAddress["/mouthClose"] * -1,
					BabbleOSC.MouthShapesWithAddress["/jawForward"]);
				_mouth.Tongue = new float3(
					0f,
					0f,
					BabbleOSC.MouthShapesWithAddress["/tongueOut"]);

				_mouth.JawOpen = BabbleOSC.MouthShapesWithAddress["/jawOpen"];
				_mouth.MouthPout = BabbleOSC.MouthShapesWithAddress["/mouthPucker"] - BabbleOSC.MouthShapesWithAddress["/mouthFunnel"];
				_mouth.TongueRoll = 0f;

				_mouth.LipBottomOverUnder = BabbleOSC.MouthShapesWithAddress["/mouthRollLower"] * -1;
				_mouth.LipBottomOverturn = 0f;
				_mouth.LipTopOverUnder = BabbleOSC.MouthShapesWithAddress["/mouthRollUpper"] * -1;
				_mouth.LipTopOverturn = 0f;

				// Assuming a tug face like this? => 0_0
				_mouth.LipLowerHorizontal = BabbleOSC.MouthShapesWithAddress["/mouthStretch_L"] - BabbleOSC.MouthShapesWithAddress["/mouthStretch_R"];
				_mouth.LipUpperHorizontal = BabbleOSC.MouthShapesWithAddress["/mouthDimple_L"] - BabbleOSC.MouthShapesWithAddress["/mouthDimple_R"];

				_mouth.LipLowerLeftRaise = BabbleOSC.MouthShapesWithAddress["/mouthLowerDown_L"];
				_mouth.LipLowerRightRaise = BabbleOSC.MouthShapesWithAddress["/mouthLowerDown_R"];
				_mouth.LipUpperRightRaise = BabbleOSC.MouthShapesWithAddress["/mouthUpperUp_R"];
				_mouth.LipUpperLeftRaise = BabbleOSC.MouthShapesWithAddress["/mouthUpperUp_L"];

				_mouth.MouthRightSmileFrown = BabbleOSC.MouthShapesWithAddress["/mouthSmile_L"] - BabbleOSC.MouthShapesWithAddress["/mouthFrown_L"];
				_mouth.MouthLeftSmileFrown = BabbleOSC.MouthShapesWithAddress["/mouthSmile_R"] - BabbleOSC.MouthShapesWithAddress["/mouthFrown_R"];
				_mouth.CheekLeftPuffSuck = BabbleOSC.MouthShapesWithAddress["/cheekPuff"];
				_mouth.CheekRightPuffSuck = BabbleOSC.MouthShapesWithAddress["/cheekPuff"];
			}

		}
	}
}
