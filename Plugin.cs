using BepInEx;
using BepInEx.Logging;
using Gameplay;
using Gameplay.Abilities;
using Gameplay.Fighters.Data;
using Gameplay.Fighters;
using Gameplay.Items;
using Gameplay.Items.Data;
using Gameplay.Items.Settings;
using Gameplay.Values;
using HarmonyLib;
using Platforms;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Utils;
using Logger = BepInEx.Logging.Logger;

namespace EverythingIsAMagnetPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class EverythingIsAMagnetPlugin : BaseUnityPlugin
{
	public static readonly Harmony Harmony = new Harmony("EverythingIsAMagnetPlugin");
    private void Awake()
    {
		EverythingIsAMagnetPlugin.Harmony.PatchAll();
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

}
[HarmonyPatch(typeof(PickupItem))]
public static class MyPatch
{
	static bool AUTOBATTLE = false;
	static bool DEBUG = false;
	static ManualLogSource myLogSource;
	static MyPatch()
	{
		if(DEBUG)
		{
			myLogSource = new ManualLogSource("EverythingIsAMagnetPluginLogSource");
			Logger.Sources.Add(myLogSource);
			myLogSource.LogInfo("TEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEST");
		}
		
	}

	[HarmonyPatch("InitProximityEffects")]
	[HarmonyPrefix]
	static bool InitProximityEffects(PickupItem __instance)
	{
		
		
		if (DEBUG) myLogSource.LogInfo("A");
		FieldInfo data = __instance.GetType().GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
		if (DEBUG) myLogSource.LogInfo("B");
		if (DEBUG) myLogSource.LogInfo("C");

		if (DEBUG) myLogSource.LogInfo("ItemName: " + (data.GetValue(__instance) as PickupItemData).GetName());
		if (DEBUG) for (int a = 0; a < (data.GetValue(__instance) as PickupItemData).BaseValues.Count; a++) myLogSource.LogInfo("Name: " + (data.GetValue(__instance) as PickupItemData).BaseValues[a].Name + " Value: " + (data.GetValue(__instance) as PickupItemData).BaseValues[a].Value);

		string name = (data.GetValue(__instance) as PickupItemData).GetName();
		bool isNameFluff = name == "Fluff";
		bool isMagnet	 = name == "Magnet";
		
		bool isMetal = (data.GetValue(__instance) as PickupItemData).Setting.Material.name == "Metal";

		if (AUTOBATTLE) isMetal = false;
		
		BaseValue baseValue = new BaseValue();
		baseValue.Name = "Radius";
		baseValue.Value = 1.5f;

		
		if (!(data.GetValue(__instance) as PickupItemData).BaseValues.Exists(x=>x.Name == "Radius") && !isNameFluff && !isMetal)
		{
			(data.GetValue(__instance) as PickupItemData).BaseValues.Add(baseValue);
		}

		ProximityEffect proximityEffect1 = new ProximityEffect();
		proximityEffect1.Magnetic.Enabled = true;
		proximityEffect1.Magnetic.DistanceScale = new ItemBaseValue();
		proximityEffect1.Magnetic.Strength = new ItemBaseValue();
		proximityEffect1.Magnetic.Radius = new ItemBaseValue();

		proximityEffect1.Magnetic.Layer = 16384;
		proximityEffect1.Magnetic.ForceMode = EffectorForceMode2D.InverseLinear;

		if (DEBUG) myLogSource.LogInfo("MAGNETS ADDED AAAAA");
		if((data.GetValue(__instance) as PickupItemData).Setting.ProximityEffects.Count == 0 && !isNameFluff && !isMetal) (data.GetValue(__instance) as PickupItemData).Setting.ProximityEffects.Add(proximityEffect1);
		if (DEBUG) myLogSource.LogInfo((data.GetValue(__instance) as PickupItemData).Setting.ProximityEffects.Count +" COUNTED ");

		foreach (ProximityEffect proximityEffect in (data.GetValue(__instance) as PickupItemData).Setting.ProximityEffects)
		{
			if (proximityEffect.Magnetic.Enabled)
			{
				PointEffector2D pointEffector2D = __instance.gameObject.AddComponent<PointEffector2D>();
				pointEffector2D.useColliderMask = true;
				pointEffector2D.colliderMask = proximityEffect.Magnetic.Layer;
				pointEffector2D.forceMagnitude = -(float)((data.GetValue(__instance) as PickupItemData).BaseValues.First(x => x.Name == "Radius").Value);
				if (!isMagnet)
				{
					pointEffector2D.forceMagnitude *= 2;
				}
				pointEffector2D.forceMode = proximityEffect.Magnetic.ForceMode;
				pointEffector2D.distanceScale = (float)((data.GetValue(__instance) as PickupItemData).BaseValues.First(x => x.Name == "Radius").Value);
				CircleCollider2D circleCollider2D = __instance.gameObject.AddComponent<CircleCollider2D>();
				
				circleCollider2D.radius = (float)(data.GetValue(__instance) as PickupItemData).BaseValues.First(x=>x.Name == "Radius").Value;

				if (DEBUG) myLogSource.LogMessage("RADIUS: " + (float)(data.GetValue(__instance) as PickupItemData).BaseValues.First(x => x.Name == "Radius").Value);
				if (DEBUG) myLogSource.LogMessage("ISMETAL: " + isMetal);
				circleCollider2D.usedByEffector = true;
				circleCollider2D.isTrigger = true;
			}
		}
		return false;
	}
	
	[HarmonyPatch("UpdateProximityEffects")]
	[HarmonyPrefix]
	static bool UpdateProximityEffects(PickupItem __instance)
	{
		return true;
		//var myLogSource = new ManualLogSource("MyLogSource");
		//Logger.Sources.Add(myLogSource);
		//myLogSource.LogInfo("TEEEEEEEEEEEEEEEEEE5EEEEEEEEEEEEEST");
		//return false;
		//FieldInfo data = __instance.GetType().GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
		//using (List<ProximityEffect>.Enumerator enumerator = (data.GetValue(__instance) as PickupItemData).Setting.ProximityEffects.GetEnumerator())
		//{
		//	while (enumerator.MoveNext())
		//	{
		//		ProximityEffect proxEffect = enumerator.Current;
		//		if (proxEffect.Magnetic != null && proxEffect.Magnetic.Enabled)
		//		{
		//			float num = (float)(proxEffect.Magnetic.Radius.GetValue(Game.Instance.Data.Fighter, (data.GetValue(__instance) as PickupItemData).BaseValues) * (double)Game.Instance.Data.MagnetismMultiplier);
		//			Collider2D[] array = new Collider2D[100];
		//			int num2 = Physics2D.OverlapCircleNonAlloc(__instance.transform.position, num, array);
		//			Vector2 a = Vector2.zero;
		//			for (int i = 0; i < num2; i++)
		//			{
		//				Collider2D collider2D = array[i];
		//				if (proxEffect.Magnetic.Layer.Contains(collider2D.gameObject.layer))
		//				{
		//					FieldInfo rigidBodyA = __instance.GetType().GetField("RigidBody", BindingFlags.NonPublic | BindingFlags.Instance);
		//					Vector2 vector = collider2D.attachedRigidbody.position - (rigidBodyA.GetValue(__instance) as Rigidbody2D).position;
		//					float num3 = num;
		//					Vector2 b2 = Vector2.Lerp(Vector2.one, Vector2.zero, 1f / num3 * vector.magnitude);
		//					a += vector.normalized * b2;
		//				}
		//			}
		//			float d = (float)proxEffect.Magnetic.Strength.GetValue(Game.Instance.Data.Fighter, (data.GetValue(__instance) as PickupItemData).BaseValues);
		//			FieldInfo rigidBodyB = __instance.GetType().GetField("RigidBody", BindingFlags.NonPublic | BindingFlags.Instance);
		//			(rigidBodyB.GetValue(__instance) as Rigidbody2D).AddForce(a * d * 0.0f, ForceMode2D.Force);
		//		}
		//		if (proxEffect.ItemEffect != null && proxEffect.ItemEffect.Enabled)
		//		{
		//			float radius = (float)proxEffect.ItemEffect.Radius.GetValue(Game.Instance.Data.Fighter, (data.GetValue(__instance) as PickupItemData).BaseValues);
		//			Collider2D[] array2 = new Collider2D[100];
		//			int num4 = Physics2D.OverlapCircleNonAlloc(__instance.transform.position, radius, array2);
		//			for (int j = 0; j < num4; j++)
		//			{
		//				PickupItem component = array2[j].GetComponent<PickupItem>();
		//				if (component != null && component != __instance && proxEffect.ItemEffect.Target.IsTarget(component))
		//				{
		//					component.ApplyItemEffect((data.GetValue(__instance) as PickupItemData), proxEffect.ItemEffect.Effect, false);
		//				}
		//			}
		//		}
		//		if (proxEffect.ItemReplacement != null && proxEffect.ItemReplacement.Enabled)
		//		{
		//			float radius2 = (float)proxEffect.ItemReplacement.Radius.GetValue(Game.Instance.Data.Fighter, (data.GetValue(__instance) as PickupItemData).BaseValues);
		//			Collider2D[] array3 = new Collider2D[100];
		//			int num5 = Physics2D.OverlapCircleNonAlloc(__instance.transform.position, radius2, array3);
		//			for (int k = 0; k < num5; k++)
		//			{
		//				PickupItem component2 = array3[k].GetComponent<PickupItem>();
		//				FieldInfo dataOfComponent2 = component2.GetType().GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
		//				if (component2 != null && component2 != __instance && proxEffect.ItemReplacement.Target.IsTarget(component2) && (dataOfComponent2.GetValue(component2) as PickupItemData).Setting != proxEffect.ItemReplacement.ItemReplacement)
		//				{
		//					component2.ChangeData(proxEffect.ItemReplacement.ItemReplacement.GenerateData(false), true, false);
		//				}
		//			}
		//		}
		//		if (proxEffect.ConsumeItems != null && proxEffect.ConsumeItems.Enabled)
		//		{
		//			float radius3 = (float)proxEffect.ConsumeItems.Radius.GetValue(Game.Instance.Data.Fighter, (data.GetValue(__instance) as PickupItemData).BaseValues);
		//			Collider2D[] array4 = new Collider2D[100];
		//			int num6 = Physics2D.OverlapCircleNonAlloc(__instance.transform.position, radius3, array4);
		//			Func<BaseValue, bool> decompVarA = null;
		//			for (int l = 0; l < num6; l++)
		//			{
		//				PickupItem component3 = array4[l].GetComponent<PickupItem>();
		//				if (component3 != null && component3 != __instance && proxEffect.ConsumeItems.Target.IsTarget(component3))
		//				{
		//					if (proxEffect.ConsumeItems.ChangeBaseValues)
		//					{
		//						FieldInfo fighter = Game.Instance.Dungeon.GetType().GetField("Fighter", BindingFlags.NonPublic | BindingFlags.Instance);
		//						FieldInfo fighterData = (fighter.GetValue(Game.Instance.Dungeon) as Fighter).GetType().GetField("FighterData", BindingFlags.NonPublic | BindingFlags.Instance);
		//						double value = proxEffect.ConsumeItems.ChangeAmount.GetValue((fighterData.GetValue(fighter) as FighterData), (data.GetValue(__instance) as PickupItemData).BaseValues);
		//						IEnumerable<BaseValue> baseValues = (data.GetValue(__instance) as PickupItemData).BaseValues;
		//						Func<BaseValue, bool> predicate = decompVarA;
		//						if (predicate == null)
		//						{
		//							predicate = (decompVarA = ((BaseValue b) => b.Name == proxEffect.ConsumeItems.BaseValue));
		//						}
		//						BaseValue baseValue = baseValues.FirstOrDefault(predicate);
		//						if (baseValue != null)
		//						{
		//							if (proxEffect.ConsumeItems.Multiply)
		//							{
		//								baseValue.Value *= value;
		//							}
		//							else
		//							{
		//								baseValue.Value += value;
		//							}
		//						}
		//					}
		//					FieldInfo machineB = __instance.GetType().GetField("_machine", BindingFlags.NonPublic | BindingFlags.Instance);
		//					(machineB.GetValue(__instance) as ClawMachine).RemoveItem(component3, true);
		//					UnityEngine.Object.Destroy(component3.gameObject);
		//				}
		//			}
		//		}
		//	}
		//}
		//FieldInfo machine = __instance.GetType().GetField("_machine", BindingFlags.NonPublic | BindingFlags.Instance);
		//(machine.GetValue(__instance) as ClawMachine).ClawMachineBox.WaterLevel.ApplyEffect(__instance);
		//return false;
	}

}