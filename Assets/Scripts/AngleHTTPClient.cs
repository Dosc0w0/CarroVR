using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class AngleHttpClient : MonoBehaviour
{
    [Header("Endpoint that returns angle")]
    [Tooltip("Examples:\nhttp://server:8080/get\nhttps://api.example.com/angle")]
    public string angleUrl = "http://192.168.1.50:8080/get";

    [Header("Polling")]
    [Tooltip("Poll frequency in Hz (requests per second)")]
    public float pollHz = 20f;

    [Tooltip("Max backoff on errors (seconds)")]
    public float maxBackoffSeconds = 10f;

    [Header("Headers (optional)")]
    public string bearerToken;
    public string customHeaderKey;
    public string customHeaderValue;

    [Header("Target")]
    public WheelRotator wheelRotator;

    private float _lastAngle;
    private float _baseInterval => pollHz > 0f ? 1f / pollHz : 0.05f;

    private static readonly Regex AngleRegex =
        new Regex("\"angle\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?(?:[eE][+-]?\\d+)?)", RegexOptions.Compiled);

    private static readonly Regex ValueRegex =
        new Regex("\"value\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?(?:[eE][+-]?\\d+)?)", RegexOptions.Compiled);

    void Reset() { wheelRotator = FindObjectOfType<WheelRotator>(); }
    void OnEnable() { StartCoroutine(PollLoop()); }
    void OnDisable() { StopAllCoroutines(); }

    private IEnumerator PollLoop()
    {
        float backoff = _baseInterval;

        while (true)
        {
            if (string.IsNullOrWhiteSpace(angleUrl))
            {
                Debug.LogWarning("[AngleHttpClient] angleUrl is empty.");
                yield return new WaitForSeconds(1f);
                continue;
            }

            using (var req = UnityWebRequest.Get(angleUrl))
            {
                if (!string.IsNullOrEmpty(bearerToken))
                    req.SetRequestHeader("Authorization", "Bearer " + bearerToken);

                if (!string.IsNullOrEmpty(customHeaderKey))
                    req.SetRequestHeader(customHeaderKey, customHeaderValue ?? "");

#if UNITY_2022_1_OR_NEWER
                req.timeout = Mathf.CeilToInt(Mathf.Clamp(_baseInterval * 4f, 2f, 10f));
#else
                req.timeout = 5;
#endif
                yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                bool isError = req.result != UnityWebRequest.Result.Success;
#else
                bool isError = req.isNetworkError || req.isHttpError;
#endif
                if (!isError)
                {
                    var text = req.downloadHandler.text?.Trim();
                    if (TryExtractAngle(text, out float angle))
                    {
                        _lastAngle = angle;
                        if (wheelRotator) wheelRotator.targetAngle = angle;
                        backoff = _baseInterval;
                    }
                    else
                    {
                        Debug.LogWarning($"[AngleHttpClient] Could not parse angle from: {text}");
                        backoff = Mathf.Min(Mathf.Max(backoff * 2f, 0.25f), maxBackoffSeconds);
                    }
                }
                else
                {
                    Debug.LogWarning($"[AngleHttpClient] Request failed: {req.error}");
                    backoff = Mathf.Min(Mathf.Max(backoff * 2f, 0.5f), maxBackoffSeconds);
                }
            }

            yield return new WaitForSeconds(backoff);
        }
    }

    private static bool TryExtractAngle(string payload, out float angle)
    {
        angle = 0f;
        if (string.IsNullOrEmpty(payload)) return false;

        if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out angle))
            return true;

        var m = AngleRegex.Match(payload);
        if (m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out angle))
            return true;

        m = ValueRegex.Match(payload);
        if (m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out angle))
            return true;

        try
        {
            var msg = JsonUtility.FromJson<AngleMsg>(payload);
            if (msg != null)
            {
                if (!float.IsNaN(msg.angle)) { angle = msg.angle; return true; }
                if (!float.IsNaN(msg.value)) { angle = msg.value; return true; }
            }
        }
        catch { }

        return false;
    }

    [Serializable]
    private class AngleMsg
    {
        public float angle = float.NaN;
        public float value = float.NaN;
    }
}
