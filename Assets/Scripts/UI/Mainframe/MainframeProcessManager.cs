using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframeProcessManager : MonoBehaviour
    {
        [SerializeField, Min(1)] private int memoryLimit = 8;
        [SerializeField, Min(4)] private int barLength = 16;
        [SerializeField] private TMP_Text memoryText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Color normalColor = new Color(0.78f, 0.78f, 0.75f);
        [SerializeField] private Color warningColor = new Color(1f, 0.64f, 0.32f);

        private readonly List<MainframeLauncher.Entry> entries = new();

        public int UsedMemory { get; private set; }
        public int MemoryLimit => memoryLimit;
        public event System.Action<string, bool> ProcessVisibilityChanged;

        public void Initialize(
            int limit,
            TMP_Text memoryLabel,
            TMP_Text statusLabel,
            Color normal,
            Color warning)
        {
            memoryLimit = Mathf.Max(1, limit);
            memoryText = memoryLabel;
            statusText = statusLabel;
            normalColor = normal;
            warningColor = warning;
            RefreshDisplay("READY");
        }

        public void SetEntries(IReadOnlyList<MainframeLauncher.Entry> newEntries)
        {
            Unsubscribe();
            entries.Clear();

            if (newEntries != null)
            {
                entries.AddRange(newEntries);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                MainframeLauncher.Entry entry = entries[i];
                if (entry?.window == null)
                {
                    continue;
                }

                entry.processManager = this;
                entry.window.VisibilityChanged += HandleWindowVisibilityChanged;
            }

            RefreshDisplay("READY");
        }

        public bool TryOpen(MainframeLauncher.Entry entry)
        {
            if (entry?.window == null)
            {
                return false;
            }

            if (entry.window.IsVisible)
            {
                entry.window.ShowWindow();
                RefreshDisplay($"{entry.label} FOCUS");
                return true;
            }

            int cost = Mathf.Max(0, entry.memoryCost);
            int projectedMemory = CalculateUsedMemory() + cost;
            if (projectedMemory > memoryLimit)
            {
                RefreshDisplay($"MEMORY LIMIT: {entry.label} DENIED");
                return false;
            }

            entry.window.ShowWindow();
            RefreshDisplay($"{entry.label} LOADED");
            return true;
        }

        public bool TryOpenByLabel(string label)
        {
            MainframeLauncher.Entry entry = FindEntry(label);
            return TryOpen(entry);
        }

        public bool HideByLabel(string label)
        {
            MainframeLauncher.Entry entry = FindEntry(label);
            if (entry?.window == null)
            {
                return false;
            }

            entry.window.HideWindow();
            return true;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                MainframeLauncher.Entry entry = entries[i];
                if (entry?.window != null)
                {
                    entry.window.VisibilityChanged -= HandleWindowVisibilityChanged;
                }
            }
        }

        private void HandleWindowVisibilityChanged(MainframeWindowFrame window, bool isVisible)
        {
            MainframeLauncher.Entry entry = FindEntry(window);
            string label = entry != null ? entry.label : "WINDOW";
            RefreshDisplay(isVisible ? $"{label} LOADED" : $"{label} SUSPENDED");
            ProcessVisibilityChanged?.Invoke(label, isVisible);
        }

        private MainframeLauncher.Entry FindEntry(MainframeWindowFrame window)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                MainframeLauncher.Entry entry = entries[i];
                if (entry?.window == window)
                {
                    return entry;
                }
            }

            return null;
        }

        private MainframeLauncher.Entry FindEntry(string label)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                MainframeLauncher.Entry entry = entries[i];
                if (entry != null && string.Equals(entry.label, label, System.StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        private void RefreshDisplay(string status)
        {
            UsedMemory = CalculateUsedMemory();
            float usage = memoryLimit <= 0 ? 0f : UsedMemory / (float)memoryLimit;
            Color color = usage >= 0.8f ? warningColor : normalColor;

            if (memoryText != null)
            {
                memoryText.text = $"MEM {BuildBar(usage)} {UsedMemory}/{memoryLimit}MB";
                memoryText.color = color;
            }

            if (statusText != null)
            {
                statusText.text = status;
                statusText.color = color;
            }
        }

        private int CalculateUsedMemory()
        {
            int total = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                MainframeLauncher.Entry entry = entries[i];
                if (entry?.window != null && entry.window.IsVisible)
                {
                    total += Mathf.Max(0, entry.memoryCost);
                }
            }

            return total;
        }

        private string BuildBar(float usage)
        {
            int safeLength = Mathf.Max(4, barLength);
            int filled = Mathf.Clamp(Mathf.RoundToInt(usage * safeLength), 0, safeLength);
            return $"[{new string('#', filled)}{new string('-', safeLength - filled)}]";
        }
    }
}
