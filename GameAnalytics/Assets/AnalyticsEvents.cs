using UnityEngine;
using GameAnalyticsSDK;
using System.Collections.Generic;

namespace GameAnalyticsIntegration
{
    public static class AnalyticsEvents
    {

        public static void SendSessionStart()
        {
            if (!GameAnalytics.IsInitialized()) return;

            Debug.Log("[Analytics] Session Start");
        }

        public static void SendSessionEnd()
        {
            if (!GameAnalytics.IsInitialized()) return;
            Debug.Log("[Analytics] Session End");
        }

        public static void SendProgressionStart(string world, string level, string subLevel = null)
        {
            if (!GameAnalytics.IsInitialized()) return;

            string progression01 = world;
            string progression02 = level;
            string progression03 = subLevel;

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start,
                progression01, progression02, progression03);

            Debug.Log($"[Analytics] Progression Start: {world}/{level}/{subLevel ?? ""}");
        }

        public static void SendProgressionComplete(string world, string level, int score, float timeSeconds)
        {
            if (!GameAnalytics.IsInitialized()) return;

            string progression01 = world;
            string progression02 = level;

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete,
                progression01, progression02, null, score);

            SendDesignEvent($"level:{level}:time", (int)timeSeconds);

            Debug.Log($"[Analytics] Progression Complete: {world}/{level} - Score: {score}, Time: {timeSeconds}s");
        }

        public static void SendProgressionFail(string world, string level, string reason = null)
        {
            if (!GameAnalytics.IsInitialized()) return;

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, world, level);

            if (!string.IsNullOrEmpty(reason))
            {
                SendDesignEvent($"level:{level}:fail_reason", reason);
            }

            Debug.Log($"[Analytics] Progression Fail: {world}/{level} - Reason: {reason ?? "unknown"}");
        }

        public static void SendDesignEvent(string eventId, int value = 0)
        {
            if (!GameAnalytics.IsInitialized()) return;

            if (value != 0)
            {
                GameAnalytics.NewDesignEvent(eventId, value);
                Debug.Log($"[Analytics] Design Event: {eventId} = {value}");
            }
            else
            {
                GameAnalytics.NewDesignEvent(eventId);
                Debug.Log($"[Analytics] Design Event: {eventId}");
            }
        }

        public static void SendDesignEvent(string eventId, string value)
        {
            if (!GameAnalytics.IsInitialized()) return;

            GameAnalytics.NewDesignEvent($"{eventId}:{value}");
            Debug.Log($"[Analytics] Design Event: {eventId}:{value}");
        }

        public static void SendResourceEarned(string currency, float amount, string itemType = null, string itemId = null)
        {
            if (!GameAnalytics.IsInitialized()) return;

            GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, currency, amount, itemType, itemId);
            Debug.Log($"[Analytics] Resource Earned: {amount} {currency} (Item: {itemType}/{itemId})");
        }

        public static void SendResourceSpent(string currency, float amount, string itemType = null, string itemId = null)
        {
            if (!GameAnalytics.IsInitialized()) return;

            GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, currency, amount, itemType, itemId);
            Debug.Log($"[Analytics] Resource Spent: {amount} {currency} (Item: {itemType}/{itemId})");
        }

        public static void SendBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType)
        {
            if (!GameAnalytics.IsInitialized()) return;

            GameAnalytics.NewBusinessEvent(currency, amount, itemType, itemId, cartType);
            Debug.Log($"[Analytics] Business Event: {amount} {currency} for {itemId}");
        }

        public static void SendErrorEvent(string severity, string message, string stackTrace = null)
        {
            if (!GameAnalytics.IsInitialized()) return;

            GAErrorSeverity errorSeverity;
            switch (severity.ToLower())
            {
                case "info": errorSeverity = GAErrorSeverity.Info; break;
                case "debug": errorSeverity = GAErrorSeverity.Debug; break;
                case "warning": errorSeverity = GAErrorSeverity.Warning; break;
                case "error": errorSeverity = GAErrorSeverity.Error; break;
                case "critical": errorSeverity = GAErrorSeverity.Critical; break;
                default: errorSeverity = GAErrorSeverity.Error; break;
            }

            string fullMessage = stackTrace != null ? $"{message}\n{stackTrace}" : message;
            GameAnalytics.NewErrorEvent(errorSeverity, fullMessage);

            Debug.Log($"[Analytics] Error Event ({severity}): {message}");
        }

        public static void SendAdEvent(string action, string adType, string adSdk, string placement)
        {
            if (!GameAnalytics.IsInitialized()) return;

            GAAdAction adAction;
            switch (action.ToLower())
            {
                case "show": adAction = GAAdAction.Show; break;
                case "click": adAction = GAAdAction.Click; break;
                case "failed_show": adAction = GAAdAction.FailedShow; break;
                default: adAction = GAAdAction.Show; break;
            }

            GAAdType type;
            switch (adType.ToLower())
            {
                case "rewardedvideo": type = GAAdType.RewardedVideo; break;
                case "interstitial": type = GAAdType.Interstitial; break;
                case "banner": type = GAAdType.Banner; break;
                default: type = GAAdType.RewardedVideo; break;
            }

            GameAnalytics.NewAdEvent(adAction, type, adSdk, placement);
            Debug.Log($"[Analytics] Ad Event: {action} - {adType} on {placement}");
        }
    }
}