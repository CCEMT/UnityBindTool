using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

public class EditorCollectionWindow : OdinEditorWindow
{
   public static void EditorCollection(BindCollection bindCollection)
   {
      EditorCollectionWindow window = GetWindow<EditorCollectionWindow>("EditorCollectionWindow");
      window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 600);
      window.Init(bindCollection);
   }

   private BindCollection editorCollection;

   void Init(BindCollection bindCollection)
   {
      this.editorCollection = bindCollection;
   }
}
