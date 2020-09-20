﻿using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using OWML.ModHelper.Input;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;
using NAudio.CoreAudioApi;
using System.IO;
using OWML.Common.Menus;
using OWML.ModHelper.Menus;

namespace Clock
{
    public class OWClock : ModBehaviour
    {
        public static IModHelper Helper { get; private set; }
        private static EventFile save;
        public static bool CountUp { get; private set; }
        public static EventFile Save { get => save; set => save = value; }

        private List<string> eventListStr = new List<string>();
        private List<KeyValuePair<float, string>> eventList = new List<KeyValuePair<float, string>>();
        private Font hudFont;
        private void Start()
        {
            Save = EventFile.LoadSaveFile();
            hudFont = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular_Dynamic");
            ModHelper.Menus.PauseMenu.OnInit += AddMenuItem;
            
            ModHelper.Console.WriteLine($"My mod {nameof(Clock)} is loaded!", MessageType.Success);
            // ModHelper.Menus.PauseMenu.OnInit += AddClock;
        }

        private void AddMenuItem()
        {
            var eventMenu = ModHelper.Menus.PauseMenu.Copy("ADD EVENT");
            var openInputButton = ModHelper.Menus.PauseMenu.ResumeButton.Duplicate("ADD EVENT");
            openInputButton.OnClick += () => EventPopup();
        }
        private void LogTime()
        {
            int currentTime = Convert.ToInt32(TimeLoop.GetSecondsElapsed());
            base.ModHelper.Console.WriteLine(string.Format(": Time is {0}", currentTime));
        }
        

        private void OnGUI()
        {
            // base.ModHelper.Console.WriteLine("OnGui called");
            if (GUIMode.IsHiddenMode() || PlayerState.UsingShipComputer())
            {
                return;
            }
            Resolution currentRes = Screen.currentResolution;
            float yPos = currentRes.height - 60f;
            float xPos = currentRes.width * 4/5;
            float elapsed = TimeLoop.GetSecondsElapsed();
            if (elapsed < 1f)
            {
                return;
            }
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.font = hudFont;
            style.fontSize = 30;
            if (CountUp)
            {
                GUI.Label(new Rect(xPos, yPos, 200f, 60f), string.Concat(new object[]
                {
                    "Time Elapsed: ",
                    ParseTime(elapsed)
                }), style) ;
            }
            else
            {
                float remaining = TimeLoop.GetSecondsRemaining();
                GUI.Label(new Rect(xPos, yPos, 200f, 60f), string.Concat(new object[]
                {
                    "Time Remaining: ",
                    ParseTime(remaining)
                }), style) ;
            }
            int count = 0;
            style.fontSize = 20;
            foreach (TimeEvent timeEvent in Save.eventList)
            {
                if (count > 5) {
                    break;
                }
                if (timeEvent.Timestamp < elapsed)
                {
                    continue;
                }
                float scaleFactor = (timeEvent.Timestamp - elapsed) / 20;
                style.normal.textColor = Color.Lerp(Color.red, Color.white, scaleFactor);
                count++;
                yPos -= 20;
                GUI.Label(new Rect(xPos, yPos, 200f, 20f), string.Concat(new object[]
                {
                    ParseTime(timeEvent.Timestamp),
                    " - ",
                    timeEvent.Name
                }), style);
            }

        }

        private void EventPopup()
        {
            var popup = ModHelper.Menus.PopupManager.CreateInputPopup(InputType.Text, "Event Name");
            popup.OnConfirm += AddEvent;

        }

        private void AddEvent(string text)
        {
            Save.AddEvent(TimeLoop.GetSecondsElapsed(), text);
        }

        string ParseTime(float timestamp)
        {
            string minutes = Mathf.Floor(timestamp / 60f).ToString().PadLeft(2, '0');
            string seconds = Mathf.Round(timestamp % 60f * 100f / 100f).ToString().PadLeft(2, '0');
            return string.Concat(new object[]
            {
                minutes,
                ":",
                seconds
            });
        }

        public override void Configure(IModConfig config)
        {
            CountUp = config.GetSettingsValue<bool>("Count Up");
            Helper = ModHelper;
        }

    }

}