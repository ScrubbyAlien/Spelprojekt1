using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeMatrix : MonoBehaviour
{
    public string matrixID;
    public string displayName;

    private List<UpgradeModule> modules;

    private Dictionary<string, (float b, float u)> upgradeDict;
    private Dictionary<string, bool> recalculated;

    private bool initialized;

    private void Start() {
        modules = new();
        upgradeDict = new();
        recalculated = new();
        initialized = true;
    }

    private void Update() {
        // recalculate = false;
        // // if the number of modules in the matrix has changed, we recalculate
        // if (modulesLastFrame.Count != modules.Count) recalculate = true;
        // else {
        //     for (int i = 0; i < modules.Count; i++) {
        //         if (!modules[i] || !modulesLastFrame[i]) break;
        //         // if at least one module does not match the last frame, we recalculate
        //         if (!modulesLastFrame[i].Equals(modules[i])) {
        //             recalculate = true;
        //             break;
        //         }
        //     }
        // }
        // modulesLastFrame.Clear();
        // for (int i = 0; i < modules.Count; i++) {
        //     modulesLastFrame.Add(modules[i]);
        // }
    }

    public float GetUpgradeValue(string id, float baseValue, bool highGood) {
        if (!initialized) return baseValue;

        // if this value has not been seen before we add recalculation check
        recalculated.TryAdd(id, false);

        // if the modules list is unchanged and the attribute has been calculated, retrieve from dictionary
        if (upgradeDict.ContainsKey(id) && recalculated[id]) {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            // if the baseValue has changed, eg. from the inspector, then we recalculate
            if (upgradeDict[id].b != baseValue) {
                recalculated[id] = false;
                return GetUpgradeValue(id, baseValue, highGood);
            }
        }

        // find all modules that modify attribute with id, and get their upgrade function
        Func<float, float>[] upgradeFunctions = modules
                                                .Where(m => m.attributeID == id)
                                                .Select(m => m.UpgradeFunction()).ToArray();

        // get all the modules upgraded values for this attribute
        List<float> upgradeValues = new() { baseValue };
        foreach (Func<float, float> upgrade in upgradeFunctions) {
            upgradeValues.Add(upgrade(baseValue));
        }

        // pick the best value
        float ret;
        if (highGood) {
            ret = upgradeValues.Max();
        }
        else {
            ret = upgradeValues.Min();
        }

        // add to dictionary for faster retrieval
        if (!upgradeDict.TryAdd(id, (baseValue, ret))) {
            upgradeDict[id] = (baseValue, ret);
        }
        recalculated[id] = true;
        return ret;
    }

    public bool AttachModule(UpgradeModule module) {
        if (!modules.Contains(module)) {
            modules.Add(module);
            CallForRecalculate();
            return true;
        }
        return false;
    }

    public bool RemoveModule(UpgradeModule module) {
        if (modules.Contains(module)) {
            modules.Remove(module);
            CallForRecalculate();
            return true;
        }
        return false;
    }

    private void CallForRecalculate() {
        foreach (string s in recalculated.Keys.ToArray()) {
            recalculated[s] = false;
        }
    }
}