using UnityEditor;

namespace AM.Editor.Menu
{
    public class ScriptTemplateWindow : BaseTemplateWindow
    {
        [MenuItem("Assets/Create/Script Template", priority = -220)]
        private static void Menu()
        {
            Open<ScriptTemplateWindow>();
        }

        protected override void BuildCategories()
        {
            RootCategories.Clear();

            var scriptCategory = new ItemCategory { Name = "Script" };

            scriptCategory.Items.Add(new TemplateItem
            {
                Name = "MonoBehaviour Script",
                Description = "A Unity component script that inherits from MonoBehaviour.",
                CategoryTag = "Unity",
                OnCreate = name => CreateScriptFile(name, ScriptUtilities.GetMonoBehaviourTemplate(name))
            });
            scriptCategory.Items.Add(new TemplateItem
            {
                Name = "ScriptableObject",
                Description = "ScriptableObject Script for data",
                CategoryTag = "Unity",
                OnCreate = name => CreateScriptFile(name, ScriptUtilities.GetScriptableObjectTemplate(name))
            });
            scriptCategory.Items.Add(new TemplateItem
            {
                Name = "Editor Script",
                Description = "A custom Inspector or EditorWindow script.",
                CategoryTag = "Unity Editor",
                OnCreate = name => CreateScriptFile(name, ScriptUtilities.GetEditorScriptTemplate(name))
            });
            scriptCategory.Items.Add(new TemplateItem
            {
                Name = "Class",
                Description = "An empty C# class definition.",
                CategoryTag = "C#",
                OnCreate = name => CreateScriptFile(name, ScriptUtilities.GetGenericTemplate(name))
            });
            scriptCategory.Items.Add(new TemplateItem
            {
                Name = "Struct",
                Description = "An empty C# struct definition.",
                CategoryTag = "C#",
                OnCreate = name => CreateScriptFile(name, ScriptUtilities.GetStructTemplate(name))
            });
            scriptCategory.Items.Add(new TemplateItem
            {
                Name = "Interface",
                Description = "An empty C# interface definition.",
                CategoryTag = "C#",
                OnCreate = name => CreateScriptFile(name, ScriptUtilities.GetInterfaceTemplate(name))
            });
            scriptCategory.Items.Add(new TemplateItem
            {
                Name = "Abstract Class",
                Description = "An empty C# abstract class definition.",
                CategoryTag = "C#",
                OnCreate = name => CreateScriptFile(name, ScriptUtilities.GetAbstractClassTemplate(name))
            });

            RootCategories.Add(scriptCategory);

            SelectedCategory = scriptCategory;
            if (scriptCategory.Items.Count > 0)
            {
                SelectedItem = scriptCategory.Items[0];
                ItemName = SelectedItem.Name.Replace(" ", "");
            }
        }
    }
}