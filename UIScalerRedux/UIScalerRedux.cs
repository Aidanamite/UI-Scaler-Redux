using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Globalization;
using HarmonyLib;
using HMLLibrary;
using RaftModLoader;

public class UIScalerRedux : Mod
{
    Harmony harmony;
    static float scale = 1;
    static Dictionary<CanvasScaler, Vector2> scalers;
    static UIScalerRedux instance;
    public void Start()
    {
        instance = this;
        scalers = new Dictionary<CanvasScaler, Vector2>();
        harmony = new Harmony("com.aidanamite.UIScalerRedux");
        harmony.PatchAll();
        foreach (var canvas in Resources.FindObjectsOfTypeAll<CanvasScaler>())
            scalers.Add(canvas, canvas.referenceResolution);
        SceneManager.sceneLoaded += OnSceneEvent;
        Log("Mod has been loaded!");
        if (ExtraSettingsAPI_Loaded)
            ExtraSettingsAPI_SettingsClose();
    }

    void OnSceneEvent(Scene scene, LoadSceneMode mode)
    {
        foreach (var canvas in Resources.FindObjectsOfTypeAll<CanvasScaler>())
            if (scalers.TryAdd(canvas, canvas.referenceResolution))
                canvas.referenceResolution *= scale;
            else
                canvas.referenceResolution = scalers[canvas] * scale;
    }

    [ConsoleCommand(name: "resetUIScale", docs: "Resets the UI scale. Best used if you set it too large to access the settings")]
    public static string MyCommand(string[] args)
    {
        ExtraSettingsAPI_SetInputValue("scale", "1");
        instance.ExtraSettingsAPI_SettingsClose();
        return "";
    }

    public void OnModUnload()
    {
        foreach (var canvas in scalers)
            canvas.Key.referenceResolution = canvas.Value;
        harmony.UnpatchAll(harmony.Id);
        Log("Mod has been unloaded!");
    }
    static bool ExtraSettingsAPI_Loaded = false;

    public void ExtraSettingsAPI_Load() => ExtraSettingsAPI_SettingsClose();
    public void ExtraSettingsAPI_SettingsClose()
    {
        try
        {
            scale = 1 / Parse(ExtraSettingsAPI_GetInputValue("scale"));
        }
        catch
        {
            scale = 1;
        }
        foreach (var e in scalers)
            e.Key.referenceResolution = e.Value * scale;
    }

    public static string ExtraSettingsAPI_GetInputValue(string SettingName) => "";
    public static void ExtraSettingsAPI_SetInputValue(string SettingName, string value) { }

    static float Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 1;
        if (value.Contains(",") && !value.Contains("."))
            value = value.Replace(',', '.');
        var c = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-NZ");
        System.Exception e = null;
        float r = 0;
        try
        {
            r = float.Parse(value);
        }
        catch (System.Exception e2)
        {
            e = e2;
        }
        CultureInfo.CurrentCulture = c;
        if (e != null)
            throw e;
        return r;
    }
}