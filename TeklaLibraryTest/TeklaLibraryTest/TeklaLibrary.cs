using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using TSMUI = Tekla.Structures.Model.UI;

/// <summary>
/// This library contains some useful method for Tekla open API
/// Author: Manuprasad M
/// Company: Aarbee Structures Pvt. Ltd.
/// Version: 1.0.20180419.16.03
/// </summary>
public class TeklaLibrary
{
    private static Model _currentModel;
    private static TransformationPlane _currentPlane;
    private static TSMUI.Picker _picker;
    private static ProgressBar _progressBar;

    static TeklaLibrary()
    {
        _currentModel = new Model();
        _currentPlane = GetWorkPlane();
        _picker = new TSMUI.Picker();
        _progressBar = new ProgressBar();
    }

    /// <summary>
    /// Create a new model and return
    /// </summary>
    /// <returns></returns>
    public static Model GetModel()
    {
        return _currentModel;
    }

    /// <summary>
    /// Return current transformation plane
    /// </summary>
    /// <returns></returns>
    public static TransformationPlane GetWorkPlane()
    {
        return _currentModel.GetWorkPlaneHandler()
            .GetCurrentTransformationPlane();
    }

    /// <summary>
    /// Reset workplane to the default plane
    /// </summary>
    public static void ResetWorkPlane()
    {
        SetWorkPlane(new TransformationPlane());
    }

    /// <summary>
    /// Set Workplane to the given plane and refresh the objects passed
    /// </summary>
    /// <param name="plane"></param>
    /// <param name="modelObjects"></param>
    public static void SetWorkPlane(TransformationPlane plane, params ModelObject[] modelObjects)
    {
        _currentModel.GetWorkPlaneHandler()
            .SetCurrentTransformationPlane(plane);

        foreach (var mObject in modelObjects)
        {
            if (mObject is null) continue;

            mObject.Select();
        }
    }

    /// <summary>
    /// Set Workplane to the given coordinate system and refresh the objects passed
    /// </summary>
    /// <param name="coordinateSystem"></param>
    /// <param name="modelObjects"></param>
    public static void SetWorkPlane(CoordinateSystem coordinateSystem, params ModelObject[] modelObjects)
    {
        SetWorkPlane(new TransformationPlane(coordinateSystem), modelObjects);
    }

    /// <summary>
    /// Save current model.
    /// If required to terminate the application then pass TRUE value.
    /// </summary>
    /// <param name="TerminateApplication"></param>
    public static void SaveModel(bool TerminateApplication = false)
    {
        SetWorkPlane(_currentPlane);

        _currentModel.CommitChanges();

        if (TerminateApplication)
            Environment.Exit(0);
    }

    /// <summary>
    /// Pick one object of given type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <returns></returns>
    public static T PickOneObject<T>(string message = null)
    {
        object obj = null;
        var prompt = message ?? "Pick one " + typeof(T).Name.ToLower();

        var warning = string.Empty;

        do
        {
            var modelObject = _picker.PickObject(TSMUI.Picker.PickObjectEnum
           .PICK_ONE_OBJECT, warning + prompt);

            if (modelObject.GetType() != (typeof(T)))
            { warning = "Object not found: "; continue; }

            obj = Convert.ChangeType(modelObject, typeof(T));
        } while (obj is null);

        return (T)obj;
    }

    /// <summary>
    /// Return the model objects of given type from model
    /// True for selected only. Progressbar will report progress
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selectedOnly"></param>
    /// <param name="progressBar"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetObjects<T>(bool selectedOnly = true, ProgressBar progressBar = null)
    {
        if (progressBar is null)
            progressBar = new ProgressBar();

        var objectEnumerator = selectedOnly ? new TSMUI.ModelObjectSelector().GetSelectedObjects() :
            _currentModel.GetModelObjectSelector().GetAllObjectsWithType(new Type[] { typeof(T) });

        progressBar.Value = 0;
        progressBar.Maximum = objectEnumerator.GetSize();

        while (objectEnumerator.MoveNext())
        {
            progressBar.Increment(1);

            if (objectEnumerator.Current.GetType() != typeof(T))
                continue;

            yield return (T)Convert.ChangeType
                (objectEnumerator.Current, typeof(T));
        }
    }

    /// <summary>
    /// Return the model objects of given type from model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selectedOnly"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetObjects<T>(bool selectedOnly)
    {
        return GetObjects<T>(selectedOnly, new ProgressBar());
    }

    /// <summary>
    /// Return the model objects of given type from model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="progressBar"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetObjects<T>(ProgressBar progressBar)
    {
        return GetObjects<T>(true, progressBar);
    }
}

public static class TeklaExtensions
{
    static TeklaExtensions()
    {
        //Not implemented
    }

    /// <summary>
    /// Return exact model name
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static string GetModelName(this Model model)
    {
        return Path.GetFileNameWithoutExtension(
            model.GetInfo().ModelName);
    }

    /// <summary>
    /// Return model path
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static string GetModelPath(this Model model)
    {
        return model.GetInfo().ModelPath;
    }

    /// <summary>
    /// Return Report folder path
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static string GetReportPath(this Model model)
    {
        return Path.Combine(model.GetInfo().ModelPath, "Reports");
    }

    /// <summary>
    /// Return Attributes folder path
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static string GetAttributesPath(this Model model)
    {
        return Path.Combine(model.GetInfo().ModelPath, "attributes");
    }

    /// <summary>
    /// Return subfolder path including model path
    /// </summary>
    /// <param name="model"></param>
    /// <param name="folders"></param>
    /// <returns></returns>
    public static string CombinePath(this Model model, params string[] folders)
    {
        return Path.Combine(model.GetModelPath(), Path.Combine(folders));
    }
}