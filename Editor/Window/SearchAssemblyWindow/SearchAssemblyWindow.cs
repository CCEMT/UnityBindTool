#region Using

using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

#endregion

namespace BindTool
{
    public class SearchAssemblyWindow : EditorWindow
    {
        private Event currentEvent;

        public Action<bool, string> callBack;
        public string inputString;

        private Vector2 typeSelectScrollPosition1;

        private List<Assembly> componentTypeList;
        private int componentAmount;

        private List<Assembly> selectList;
        private int selectAmount;
        private int selectIndex;

        public static void ShowWindown(Action<bool, string> callBack, Vector2? position)
        {
            Vector2 targetPosition = Vector2.zero;
            if (position != null) targetPosition = (Vector2) position;

            SearchAssemblyWindow selectWindown = GetWindowWithRect<SearchAssemblyWindow>(new Rect(targetPosition.x, targetPosition.y, 500, 600), true, "Select Type");
            selectWindown.position = new Rect(targetPosition.x, targetPosition.y, 500, 600);
            selectWindown.callBack = callBack;

            selectWindown.inputString = "";
            selectWindown.currentEvent = Event.current;

            selectWindown.componentTypeList = new List<Assembly>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            selectWindown.componentTypeList.AddRange(assemblies);

            selectWindown.componentAmount = selectWindown.componentTypeList.Count;
            selectWindown.GetSelectList();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            InputControl();
            WindownShow();
        }

        void WindownShow()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Label("Select Type：");
                GUI.SetNextControlName("Input");
                string tempString = GUILayout.TextField(inputString, "SearchTextField");
                if (tempString.Equals(inputString) == false)
                {
                    inputString = tempString;
                    GetSelectList();
                }
                GUI.FocusControl("Input");
            }
            GUILayout.EndHorizontal();

            float height = 525f;

            GUILayout.BeginVertical("box");
            {
                typeSelectScrollPosition1 = EditorGUILayout.BeginScrollView(typeSelectScrollPosition1, false, false, GUILayout.ExpandWidth(true), GUILayout.Height(height));

                for (int i = 0; i < selectAmount; i++)
                {
                    if (selectIndex == i) GUI.color = Color.green;
                    GUILayout.BeginVertical("box");
                    {
                        Assembly assembly = selectList[i];
                        if (GUILayout.Button($"{assembly.GetName().Name}", GUILayout.Width(457.5f)))
                        {
                            Close();
                            callBack?.Invoke(true, assembly.GetName().Name);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;
                }

                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        void GetSelectList()
        {
            selectList = new List<Assembly>();
            selectIndex = 0;
            for (int i = 0; i < componentAmount; i++)
            {
                Assembly assembly = componentTypeList[i];
                if (FuzzySearch.Contains(assembly.FullName, inputString)) selectList.Add(assembly);
            }
            selectAmount = selectList.Count;
        }

        void InputControl()
        {
            float height = 525f;
            float itmeHeight = 525f / 18f;
            float currentHeight = 0;

            if (this.currentEvent.type != EventType.KeyDown) return;
            switch (this.currentEvent.keyCode)
            {
                case KeyCode.UpArrow:
                    this.selectIndex -= 1;
                    this.selectIndex = Mathf.Clamp(this.selectIndex, 0, this.selectAmount);
                    Repaint();
                    this.currentEvent.Use();
                    currentHeight = itmeHeight * this.selectIndex;
                    if (currentHeight < this.typeSelectScrollPosition1.y) this.typeSelectScrollPosition1.y -= itmeHeight;
                    break;
                case KeyCode.DownArrow:
                    this.selectIndex += 1;
                    this.selectIndex = Mathf.Clamp(this.selectIndex, 0, this.selectAmount - 1);
                    Repaint();
                    this.currentEvent.Use();
                    currentHeight = itmeHeight * (this.selectIndex + 1);
                    if (currentHeight > this.typeSelectScrollPosition1.y + height) this.typeSelectScrollPosition1.y += itmeHeight;
                    break;
                case KeyCode.Return:
                    if (this.selectList.Count > 0)
                    {
                        this.callBack?.Invoke(true, this.selectList[this.selectIndex].GetName().Name);
                        Close();
                    }
                    this.currentEvent.Use();
                    break;
            }
        }
    }
}