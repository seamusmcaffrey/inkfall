using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decorative lane visuals and remaining-darts pips.
/// Builds a neon noir launch lane with floor panel, guide lines, and dashed center line.
/// </summary>
[DisallowMultipleComponent]
public class LaunchLaneVisuals : MonoBehaviour
{
    private const float LANE_HALF_WIDTH = 1.7f;
    private const float GUIDE_LINE_WIDTH = 0.035f;
    private const float CENTER_DASH_LENGTH = 0.35f;
    private const float CENTER_GAP_LENGTH = 0.25f;
    private const float CENTER_LINE_WIDTH = 0.025f;
    private const float LINE_Z = -0.5f;

    private readonly List<SpriteRenderer> _dartPips = new();

    private void OnEnable()
    {
        EventBus.Subscribe<DartsRemainingChangedEvent>(HandleDartsChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DartsRemainingChangedEvent>(HandleDartsChanged);
    }

    private void Awake()
    {
        BuildDartPips();
        BuildGuideLine("GuideLineLeft", -LANE_HALF_WIDTH);
        BuildGuideLine("GuideLineRight", LANE_HALF_WIDTH);
        BuildDashedCenterLine();
    }

    // ── Dart pips (existing functionality) ──────────────────────────────

    private void BuildDartPips()
    {
        for (int index = 0; index < GameConstants.STARTING_DARTS + 4; index++)
        {
            GameObject pip = new($"DartPip_{index}");
            pip.transform.SetParent(transform, false);
            pip.transform.localPosition = new Vector3(-1.2f + index * 0.35f, -1.4f, 0f);
            SpriteRenderer renderer = pip.AddComponent<SpriteRenderer>();
            renderer.color = UIColors.DartBlue;
            _dartPips.Add(renderer);
        }
    }

    private void HandleDartsChanged(DartsRemainingChangedEvent evt)
    {
        for (int index = 0; index < _dartPips.Count; index++)
        {
            _dartPips[index].color = index < evt.DartsRemaining ? UIColors.DartBlue : new Color(1f, 1f, 1f, 0.12f);
        }
    }

    // ── Guide lines along lane edges ────────────────────────────────────

    private void BuildGuideLine(string name, float xPosition)
    {
        float laneTop = GameConstants.LANE_TOP;
        float laneBottom = GameConstants.LANE_BOTTOM;

        GameObject lineObj = new(name);
        lineObj.transform.SetParent(transform, false);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        Color guideColor = new(UIColors.InkCyan.r, UIColors.InkCyan.g, UIColors.InkCyan.b, 0.15f);
        Color guideFade = new(UIColors.InkCyan.r, UIColors.InkCyan.g, UIColors.InkCyan.b, 0.04f);

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = guideFade;
        line.endColor = guideColor;
        line.startWidth = GUIDE_LINE_WIDTH;
        line.endWidth = GUIDE_LINE_WIDTH;
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.SetPosition(0, new Vector3(xPosition, laneTop, LINE_Z));
        line.SetPosition(1, new Vector3(xPosition, laneBottom, LINE_Z));
    }

    // ── Dashed center line ──────────────────────────────────────────────

    private void BuildDashedCenterLine()
    {
        float laneTop = GameConstants.LANE_TOP;
        float laneBottom = GameConstants.LANE_BOTTOM;
        float y = laneTop;
        int dashIndex = 0;

        Color dashColor = new(UIColors.DartBlue.r, UIColors.DartBlue.g, UIColors.DartBlue.b, 0.2f);

        while (y - CENTER_DASH_LENGTH > laneBottom)
        {
            float dashStart = y;
            float dashEnd = Mathf.Max(y - CENTER_DASH_LENGTH, laneBottom);

            GameObject dashObj = new($"CenterDash_{dashIndex}");
            dashObj.transform.SetParent(transform, false);

            LineRenderer dash = dashObj.AddComponent<LineRenderer>();
            dash.material = new Material(Shader.Find("Sprites/Default"));
            dash.startColor = dashColor;
            dash.endColor = dashColor;
            dash.startWidth = CENTER_LINE_WIDTH;
            dash.endWidth = CENTER_LINE_WIDTH;
            dash.positionCount = 2;
            dash.useWorldSpace = true;
            dash.SetPosition(0, new Vector3(0f, dashStart, LINE_Z));
            dash.SetPosition(1, new Vector3(0f, dashEnd, LINE_Z));

            y -= CENTER_DASH_LENGTH + CENTER_GAP_LENGTH;
            dashIndex++;
        }
    }

}
