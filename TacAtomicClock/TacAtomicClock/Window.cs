using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

abstract class Window
{
    private bool visible = false;
    private Rect windowPos = new Rect(60, 60, 60, 60);
    private string windowTitle;
    private PartModule partModule;
    private int windowId;

    protected Window(string windowTitle, PartModule partModule)
    {
        this.windowTitle = windowTitle;
        this.partModule = partModule;
        this.windowId = windowTitle.GetHashCode() + new System.Random().Next(65536);
    }

    public bool IsVisible()
    {
        return visible;
    }

    public virtual void SetVisible(bool newValue)
    {
        if (newValue)
        {
            if (!visible)
            {
                RenderingManager.AddToPostDrawQueue(3, new Callback(CreateWindow));
            }
        }
        else
        {
            if (visible)
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(CreateWindow));
            }
        }

        this.visible = newValue;
    }

    public void SetSize(int width, int height)
    {
        windowPos.width = width;
        windowPos.height = height;
    }

    public virtual void Load(ConfigNode config, string subnode)
    {
        Debug.Log("TAC Atomic Clock [" + Time.time + "]: Load " + subnode);
        if (config.HasNode(subnode))
        {
            ConfigNode windowConfig = config.GetNode(subnode);

            float newValue;
            if (windowConfig.HasValue("xPos") && float.TryParse(windowConfig.GetValue("xPos"), out newValue))
            {
                windowPos.xMin = newValue;
            }

            if (windowConfig.HasValue("yPos") && float.TryParse(windowConfig.GetValue("yPos"), out newValue))
            {
                windowPos.yMin = newValue;
            }
        }
    }

    public virtual void Save(ConfigNode config, string subnode)
    {
        Debug.Log("TAC Atomic Clock [" + Time.time + "]: Save " + subnode);

        ConfigNode windowConfig;
        if (config.HasNode(subnode))
        {
            windowConfig = config.GetNode(subnode);
        }
        else
        {
            windowConfig = new ConfigNode(subnode);
            config.AddNode(windowConfig);
        }

        windowConfig.AddValue("xPos", windowPos.xMin);
        windowConfig.AddValue("yPos", windowPos.yMin);
    }

    protected virtual void CreateWindow()
    {
        try
        {
            if (partModule.part.State != PartStates.DEAD && partModule.vessel.isActiveVessel)
            {
                GUI.skin = HighLogic.Skin;
                windowPos = GUILayout.Window(windowId, windowPos, Draw, windowTitle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            else
            {
                SetVisible(false);
            }
        }
        catch
        {
            SetVisible(false);
        }
    }

    protected abstract void Draw(int windowID);
}
