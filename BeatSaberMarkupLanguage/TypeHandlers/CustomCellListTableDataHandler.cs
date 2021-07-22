﻿using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static BeatSaberMarkupLanguage.BSMLParser;
using static HMUI.TableView;

namespace BeatSaberMarkupLanguage.TypeHandlers
{
    [ComponentHandler(typeof(CustomCellListTableData))]
    public class CustomCellListTableDataHandler : TypeHandler
    {
        // We need to use hard reflection like this because ScrollView's direction enum is private.
        private readonly FieldInfo scrollViewDirectionField = typeof(ScrollView)
             .GetField("_scrollViewDirection", BindingFlags.Instance | BindingFlags.NonPublic);

        public override Dictionary<string, string[]> Props => new Dictionary<string, string[]>()
        {
            { "selectCell", new[]{ "select-cell" } },
            { "visibleCells", new[]{ "visible-cells"} },
            { "cellSize", new[]{ "cell-size"} },
            { "id", new[]{ "id" } },
            { "listWidth", new[] { "list-width" } },
            { "listHeight", new[] { "list-height" } },
            { "listDirection", new[] { "list-direction" } },
            { "data", new[] { "contents", "data" } },
            { "cellClickable", new[] { "clickable-cells" } },
            { "cellTemplate", new[] { "_children" } },
            { "alignCenter", new[] { "align-to-center" } }
        };

        public override void HandleType(ComponentTypeWithData componentType, BSMLParserParams parserParams)
        {
            CustomCellListTableData tableData = componentType.component as CustomCellListTableData;
            if (componentType.data.TryGetValue("selectCell", out string selectCell))
            {
                tableData.tableView.didSelectCellWithIdxEvent += delegate (TableView table, int index)
                {
                    if (!parserParams.actions.TryGetValue(selectCell, out BSMLAction action))
                        throw new Exception("select-cell action '" + componentType.data["selectCell"] + "' not found");

                    action.Invoke(table, (table.dataSource as CustomCellListTableData).data[index]);
                };
            }

            if (componentType.data.TryGetValue("listDirection", out string listDirection))
            {
                tableData.tableView.SetField<TableView, TableType>("_tableType", (TableType)Enum.Parse(typeof(TableType), listDirection));
            
                var scrollView = tableData.tableView.GetField<ScrollView, TableView>("_scrollView");

                scrollViewDirectionField.SetValue(scrollView, Enum.Parse(scrollViewDirectionField.FieldType, listDirection));
            }

            if (componentType.data.TryGetValue("cellSize", out string cellSize))
                tableData.cellSize = Parse.Float(cellSize);

            if (componentType.data.TryGetValue("cellTemplate", out string cellTemplate))
                tableData.cellTemplate = "<bg>"+cellTemplate+"</bg>";

            if (componentType.data.TryGetValue("cellClickable", out string cellClickable))
                tableData.clickableCells = Parse.Bool(cellClickable);

            if (componentType.data.TryGetValue("alignCenter", out string alignCenter))
                tableData.tableView.SetField<TableView, bool>("_alignToCenter", Parse.Bool(alignCenter));

            if (componentType.data.TryGetValue("data", out string value))
            {
                if (!parserParams.values.TryGetValue(value, out BSMLValue contents))
                    throw new Exception("value '" + value + "' not found");
                tableData.data = contents.GetValue() as List<object>;
                tableData.tableView.ReloadData();
            }

            switch (tableData.tableView.tableType)
            {
                case TableType.Vertical:
                    (componentType.component.gameObject.transform as RectTransform).sizeDelta = new Vector2(componentType.data.TryGetValue("listWidth", out string vListWidth) ? Parse.Float(vListWidth) : 60, tableData.cellSize * (componentType.data.TryGetValue("visibleCells", out string vVisibleCells) ? Parse.Float(vVisibleCells) : 7));
                    break;
                case TableType.Horizontal:
                    (componentType.component.gameObject.transform as RectTransform).sizeDelta = new Vector2(tableData.cellSize * (componentType.data.TryGetValue("visibleCells", out string hVisibleCells) ? Parse.Float(hVisibleCells) : 4), componentType.data.TryGetValue("listHeight", out string hListHeight) ? Parse.Float(hListHeight) : 40);
                    break;
            }

            componentType.component.gameObject.GetComponent<LayoutElement>().preferredHeight = (componentType.component.gameObject.transform as RectTransform).sizeDelta.y;
            componentType.component.gameObject.GetComponent<LayoutElement>().preferredWidth = (componentType.component.gameObject.transform as RectTransform).sizeDelta.x;

            tableData.tableView.gameObject.SetActive(true);
            Utilities.InvokePrivateMethod(tableData.tableView, "LazyInit", null);

            if (componentType.data.TryGetValue("id", out string id))
            {
                ScrollView scroller = tableData.tableView.GetField<ScrollView, TableView>("_scrollView");
                parserParams.AddEvent(id + "#PageUp", Utilities.GetBindedPrivateAction(scroller, "PageUpButtonPressed"));
                parserParams.AddEvent(id + "#PageDown", Utilities.GetBindedPrivateAction(scroller, "PageDownButtonPressed"));
            }
        }
    }
}
