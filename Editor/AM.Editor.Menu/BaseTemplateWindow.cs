using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AM.Editor.Menu
{
    [Serializable]
    public class ItemCategory
    {
        public string Name;
        public List<ItemCategory> Children = new List<ItemCategory>();
        public List<TemplateItem> Items = new List<TemplateItem>();
        public bool IsExpanded = true;
    }

    [Serializable]
    public class TemplateItem
    {
        public string Name;
        public string Description;
        public Texture2D Icon;
        public string CategoryTag;
        public Action<string> OnCreate;
    }

    public abstract class BaseTemplateWindow : EditorWindow
    {
        private const float CategoryPanelWidth = 200f;
        private const float ItemNameColumnWidth = 260f;
        private const float TagColumnWidth = 100f;
        private const float BottomBarHeight = 60f;
        private const float RowHeight = 28f;
        private const float IndentWidth = 16f;

        private static readonly Color ColorBackground = new Color(0.196f, 0.196f, 0.196f);
        private static readonly Color ColorPanelBackground = new Color(0.157f, 0.157f, 0.157f);
        private static readonly Color ColorBorder = new Color(0.118f, 0.118f, 0.118f);
        private static readonly Color ColorRowSelected = new Color(0.188f, 0.361f, 0.573f);
        private static readonly Color ColorRowHover = new Color(0.255f, 0.255f, 0.255f);
        private static readonly Color ColorRowEven = new Color(0.196f, 0.196f, 0.196f);
        private static readonly Color ColorRowOdd = new Color(0.208f, 0.208f, 0.208f);
        private static readonly Color ColorText = new Color(0.878f, 0.878f, 0.878f);
        private static readonly Color ColorSubText = new Color(0.600f, 0.600f, 0.600f);
        private static readonly Color ColorBottomBar = new Color(0.173f, 0.173f, 0.173f);
        private static readonly Color ColorButton = new Color(0.267f, 0.380f, 0.502f);
        private static readonly Color ColorButtonText = Color.white;
        private static readonly Color ColorCategorySelected = new Color(0.188f, 0.361f, 0.573f);

        protected List<ItemCategory> RootCategories = new List<ItemCategory>();
        protected ItemCategory SelectedCategory;
        protected TemplateItem SelectedItem;
        protected string ItemName = "NewScript";
        private string searchText = "";

        private Vector2 categoryScrollPos;
        private Vector2 itemListScrollPos;

        private int hoveredItemIndex = -1;
        private int hoveredCategoryId = -1;

        private GUIStyle styleCategoryLabel;
        private GUIStyle styleItemLabel;
        private GUIStyle styleTagLabel;
        private GUIStyle styleDescriptionLabel;
        private GUIStyle styleSearchField;
        private GUIStyle styleNameField;
        private bool stylesInitialized;

        protected static void Open<T>() where T : BaseTemplateWindow
        {
            var window = GetWindow<T>(true, "Create New", true);
            window.minSize = new Vector2(780, 520);
            window.maxSize = new Vector2(1200, 800);
            window.Show();
        }

        private void OnEnable()
        {
            BuildCategories();
        }

        protected abstract void BuildCategories();

        private void InitStyles()
        {
            if (stylesInitialized) return;

            styleCategoryLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                normal = { textColor = ColorText },
                hover = { textColor = Color.white },
                padding = new RectOffset(4, 4, 2, 2)
            };

            styleItemLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                normal = { textColor = ColorText },
                padding = new RectOffset(6, 4, 0, 0),
                alignment = TextAnchor.MiddleLeft
            };

            styleTagLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = ColorSubText },
                padding = new RectOffset(4, 4, 0, 0),
                alignment = TextAnchor.MiddleLeft
            };

            styleDescriptionLabel = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 11,
                normal = { textColor = ColorSubText },
                padding = new RectOffset(6, 6, 4, 4)
            };

            styleSearchField = new GUIStyle(EditorStyles.toolbarSearchField)
            {
                fixedHeight = 22f
            };

            styleNameField = new GUIStyle(EditorStyles.textField)
            {
                fontSize = 12,
                fixedHeight = 22f
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), ColorBackground);

            float topBarHeight = 30f;
            float mainAreaY = topBarHeight;
            float mainAreaHeight = position.height - topBarHeight - BottomBarHeight;

            DrawTopBar(new Rect(0, 0, position.width, topBarHeight));
            DrawMainArea(new Rect(0, mainAreaY, position.width, mainAreaHeight));
            DrawBottomBar(new Rect(0, position.height - BottomBarHeight, position.width, BottomBarHeight));

            if (Event.current.type == EventType.MouseMove)
            {
                hoveredItemIndex = -1;
                Repaint();
            }
        }

        private void DrawTopBar(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorPanelBackground);
            DrawHorizontalLine(new Rect(rect.x, rect.yMax - 1, rect.width, 1));

            float labelWidth = 60f;
            float searchWidth = 200f;
            float padding = 8f;

            var labelRect = new Rect(rect.xMax - labelWidth - searchWidth - padding * 2, rect.y + 4, labelWidth, 22);
            var searchRect = new Rect(rect.xMax - searchWidth - padding, rect.y + 4, searchWidth, 22);

            EditorGUI.LabelField(labelRect, "Search(Ctrl+E)", styleTagLabel);
            searchText = EditorGUI.TextField(searchRect, searchText, styleSearchField);
        }

        private void DrawMainArea(Rect rect)
        {
            var categoryRect = new Rect(rect.x, rect.y, CategoryPanelWidth, rect.height);
            DrawCategoryPanel(categoryRect);

            DrawVerticalLine(new Rect(categoryRect.xMax, rect.y, 1, rect.height));

            var itemRect = new Rect(categoryRect.xMax + 1, rect.y, rect.width - CategoryPanelWidth - 1, rect.height);
            DrawItemListPanel(itemRect);
        }

        private void DrawCategoryPanel(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorPanelBackground);

            categoryScrollPos = GUI.BeginScrollView(
                new Rect(rect.x, rect.y, rect.width, rect.height),
                categoryScrollPos,
                new Rect(0, 0, rect.width - 16, CalculateCategoryHeight()));

            float y = 4f;
            int idCounter = 0;
            foreach (var cat in RootCategories)
            {
                y = DrawCategoryNode(cat, y, 0, ref idCounter);
            }

            GUI.EndScrollView();
        }

        private float CalculateCategoryHeight()
        {
            float h = 0;
            foreach (var cat in RootCategories)
                h += CalculateCategoryNodeHeight(cat);
            return h + 8f;
        }

        private float CalculateCategoryNodeHeight(ItemCategory cat)
        {
            float h = RowHeight;
            if (cat.IsExpanded)
                foreach (var child in cat.Children)
                    h += CalculateCategoryNodeHeight(child);
            return h;
        }

        private float DrawCategoryNode(ItemCategory cat, float y, int depth, ref int idCounter)
        {
            int myId = idCounter++;
            bool isSelected = (SelectedCategory == cat);
            bool isHovered = (hoveredCategoryId == myId);

            var rowRect = new Rect(0, y, CategoryPanelWidth - 16, RowHeight);

            if (isSelected)
                EditorGUI.DrawRect(rowRect, ColorCategorySelected);
            else if (isHovered)
                EditorGUI.DrawRect(rowRect, ColorRowHover);

            float indent = depth * IndentWidth + 8f;
            bool hasChildren = cat.Children.Count > 0;

            if (hasChildren)
            {
                var foldRect = new Rect(indent, y + 6, 16, 16);
                cat.IsExpanded = EditorGUI.Foldout(foldRect, cat.IsExpanded, GUIContent.none);
                indent += 16f;
            }
            else
            {
                indent += 16f;
            }

            var labelRect = new Rect(indent, y, CategoryPanelWidth - 16 - indent, RowHeight);
            EditorGUI.LabelField(labelRect, cat.Name, styleCategoryLabel);

            if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
            {
                SelectedCategory = cat;
                SelectedItem = null;
                hoveredItemIndex = -1;
                GUI.FocusControl(null);
                Repaint();
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseMove && rowRect.Contains(Event.current.mousePosition))
            {
                hoveredCategoryId = myId;
                Repaint();
            }

            y += RowHeight;

            if (cat.IsExpanded)
            {
                foreach (var child in cat.Children)
                    y = DrawCategoryNode(child, y, depth + 1, ref idCounter);
            }

            return y;
        }

        private void DrawItemListPanel(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorBackground);

            float headerHeight = 24f;
            DrawItemListHeader(new Rect(rect.x, rect.y, rect.width, headerHeight));
            DrawHorizontalLine(new Rect(rect.x, rect.y + headerHeight, rect.width, 1));

            float descHeight = 48f;
            float listHeight = rect.height - headerHeight - 1 - descHeight;

            var listRect = new Rect(rect.x, rect.y + headerHeight + 1, rect.width, listHeight);
            DrawItemList(listRect);

            DrawHorizontalLine(new Rect(rect.x, rect.y + headerHeight + 1 + listHeight, rect.width, 1));

            var descRect = new Rect(rect.x, rect.y + headerHeight + 1 + listHeight + 1, rect.width, descHeight - 1);
            DrawDescription(descRect);
        }

        private void DrawItemListHeader(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorPanelBackground);
            EditorGUI.LabelField(
                new Rect(rect.x + 32, rect.y, ItemNameColumnWidth, rect.height),
                "Name", styleTagLabel);
            EditorGUI.LabelField(
                new Rect(rect.x + 32 + ItemNameColumnWidth, rect.y, TagColumnWidth, rect.height),
                "Type", styleTagLabel);
        }

        private void DrawItemList(Rect rect)
        {
            var items = GetFilteredItems();
            float contentHeight = items.Count * RowHeight;

            itemListScrollPos = GUI.BeginScrollView(
                rect,
                itemListScrollPos,
                new Rect(0, 0, rect.width - 16, Mathf.Max(contentHeight, rect.height)));

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var rowRect = new Rect(0, i * RowHeight, rect.width - 16, RowHeight);
                bool isSelected = (SelectedItem == item);
                bool isHovered = (hoveredItemIndex == i);

                Color rowColor = isSelected ? ColorRowSelected
                               : isHovered ? ColorRowHover
                               : (i % 2 == 0) ? ColorRowEven : ColorRowOdd;
                EditorGUI.DrawRect(rowRect, rowColor);

                var iconRect = new Rect(4, i * RowHeight + 4, 20, 20);
                if (item.Icon != null)
                    GUI.DrawTexture(iconRect, item.Icon, ScaleMode.ScaleToFit);
                else
                {
                    EditorGUI.DrawRect(iconRect, new Color(0.4f, 0.5f, 0.6f));
                    EditorGUI.LabelField(iconRect, "◻", new GUIStyle(EditorStyles.label)
                    {
                        fontSize = 14,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    });
                }

                var nameRect = new Rect(30, i * RowHeight, ItemNameColumnWidth, RowHeight);
                EditorGUI.LabelField(nameRect, item.Name, styleItemLabel);

                var tagRect = new Rect(30 + ItemNameColumnWidth, i * RowHeight, TagColumnWidth, RowHeight);
                EditorGUI.LabelField(tagRect, item.CategoryTag ?? "", styleTagLabel);

                if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                {
                    SelectedItem = item;
                    ItemName = item.Name.Replace(" ", "");
                    GUI.FocusControl(null);
                    Repaint();

                    if (Event.current.clickCount == 2)
                        ExecuteCreate();

                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseMove && rowRect.Contains(Event.current.mousePosition))
                {
                    hoveredItemIndex = i;
                    Repaint();
                }
            }

            GUI.EndScrollView();
        }

        private void DrawDescription(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorPanelBackground);
            if (SelectedItem != null)
            {
                string desc = $"Type: {SelectedItem.CategoryTag}\n{SelectedItem.Description}";
                EditorGUI.LabelField(rect, desc, styleDescriptionLabel);
            }
        }

        private void DrawBottomBar(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorBottomBar);
            DrawHorizontalLine(new Rect(rect.x, rect.y, rect.width, 1));

            float padding = 10f;
            float labelWidth = 60f;
            float fieldWidth = 320f;
            float buttonWidth = 80f;
            float buttonHeight = 26f;
            float buttonY = rect.y + (rect.height - buttonHeight) * 0.5f;

            var labelRect = new Rect(rect.x + padding, rect.y + 16, labelWidth, 22);
            EditorGUI.LabelField(labelRect, "Name (N):", styleTagLabel);

            GUI.SetNextControlName("ItemNameField");
            var fieldRect = new Rect(rect.x + padding + labelWidth + 4, rect.y + 16, fieldWidth, 22);
            ItemName = EditorGUI.TextField(fieldRect, ItemName, styleNameField);

            var addRect = new Rect(rect.xMax - (buttonWidth + padding) * 2, buttonY, buttonWidth, buttonHeight);
            DrawButton(addRect, "Create (A)", () => ExecuteCreate());

            var cancelRect = new Rect(rect.xMax - buttonWidth - padding, buttonY, buttonWidth, buttonHeight);
            DrawButton(cancelRect, "Cancel", Close, new Color(0.3f, 0.3f, 0.3f));
        }

        private void DrawButton(Rect rect, string label, Action onClick, Color? color = null)
        {
            Color btnColor = color ?? ColorButton;
            EditorGUI.DrawRect(rect, btnColor);

            var btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                normal = { textColor = ColorButtonText, background = Texture2D.blackTexture },
                hover = { textColor = Color.white, background = Texture2D.blackTexture },
                active = { textColor = Color.white, background = Texture2D.blackTexture },
            };

            if (GUI.Button(rect, label, btnStyle))
                onClick?.Invoke();
        }

        private void DrawHorizontalLine(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorBorder);
        }

        private void DrawVerticalLine(Rect rect)
        {
            EditorGUI.DrawRect(rect, ColorBorder);
        }

        private List<TemplateItem> GetFilteredItems()
        {
            var list = new List<TemplateItem>();
            if (SelectedCategory == null) return list;

            bool hasSearch = !string.IsNullOrEmpty(searchText);

            if (hasSearch)
            {
                CollectAllItems(RootCategories, list);
                list.RemoveAll(item =>
                    !item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                list.AddRange(SelectedCategory.Items);
            }

            return list;
        }

        private void CollectAllItems(List<ItemCategory> categories, List<TemplateItem> result)
        {
            foreach (var cat in categories)
            {
                result.AddRange(cat.Items);
                CollectAllItems(cat.Children, result);
            }
        }

        private void ExecuteCreate()
        {
            if (SelectedItem == null)
            {
                EditorUtility.DisplayDialog("Warning", "Please select a type.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ItemName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a name.", "OK");
                return;
            }

            try
            {
                SelectedItem.OnCreate?.Invoke(ItemName);
                Close();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Failed to Create", e.Message, "OK");
                Debug.LogException(e);
            }
        }

        protected static void CreateScriptFile(string fileName, string content)
        {
            ScriptUtilities.GetGenerateSetting(fileName,
                out _,
                out string className,
                out _,
                out _,
                out _);

            string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(folder))
                folder = "Assets";
            else if (!System.IO.Directory.Exists(folder))
                folder = System.IO.Path.GetDirectoryName(folder);

            string path = $"{folder}/{className}.cs";
            System.IO.File.WriteAllText(path, content);
            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (asset != null)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }

            Debug.Log($"[NewItemWindow] '{path}' created successfully.");
        }
    }
}