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
    public class TypeSelectWindow : EditorWindow
    {
        private Event currentEvent;

        public Action<bool, TypeString> callBack;
        public string inputString;

        private Vector2 typeSelectScrollPosition1;

        private List<Type> componentTypeList;
        private int componentAmount;

        private List<Type> selectList;
        private int selectAmount;
        private int selectIndex;

        public static void ShowWindown(Type limitType, Action<bool, TypeString> callBack, Vector2? position)
        {
            Vector2 targetPosition = Vector2.zero;
            if (position != null) targetPosition = (Vector2) position;

            TypeSelectWindow selectWindown = GetWindowWithRect<TypeSelectWindow>(new Rect(targetPosition.x, targetPosition.y, 500, 600), true, "Select Type");
            selectWindown.position = new Rect(targetPosition.x, targetPosition.y, 500, 600);
            selectWindown.callBack = callBack;

            selectWindown.inputString = "";
            selectWindown.currentEvent = Event.current;

            //获取所有程序集下继承Component的Type
            selectWindown.componentTypeList = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            int assemblieAmount = assemblies.Length;
            for (int i = 0; i < assemblieAmount; i++)
            {
                Type[] types = assemblies[i].GetTypes();
                int typeAmount = types.Length;
                for (int j = 0; j < typeAmount; j++)
                {
                    Type type = types[j];
                    if (limitType.IsAssignableFrom(type)) selectWindown.componentTypeList.Add(type);
                }
            }

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

                        Type type = selectList[i];
                        if (GUILayout.Button($"({type.Namespace}) {type.Name}", GUILayout.Width(457.5f)))
                        {
                            Close();
                            callBack?.Invoke(true, new TypeString(type));
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
            selectList = new List<Type>();
            selectIndex = 0;
            for (int i = 0; i < componentAmount; i++)
            {
                Type type = componentTypeList[i];
                if (FuzzySearch.Contains(type.Name, inputString)) selectList.Add(type);
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
                    this.selectIndex = Mathf.Clamp(this.selectIndex, 0, this.selectAmount-1);
                    Repaint();
                    this.currentEvent.Use();
                    currentHeight = itmeHeight * (this.selectIndex + 1);
                    if (currentHeight > this.typeSelectScrollPosition1.y + height) this.typeSelectScrollPosition1.y += itmeHeight;
                    break;
                case KeyCode.Return:
                    if (this.selectList.Count > 0)
                    {
                        this.callBack?.Invoke(true, new TypeString(this.selectList[this.selectIndex]));
                        Close();
                    }
                    this.currentEvent.Use();
                    break;
            }
        }
    }
}