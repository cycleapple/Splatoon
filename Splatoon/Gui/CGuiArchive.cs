using Dalamud.Plugin.Ipc.Exceptions;
using ECommons.Configuration;
using ECommons.LanguageHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Splatoon;
internal unsafe partial class CGui
{
    internal void DrawArchive()
    {
        ImGuiEx.TextWrapped("""
            You may archive layouts that you are no longer using. Archived layouts:
            - Are not processed and do not consume any resources;
            - Are included into backups;
            - Can not be edited, viewed, reordered;
            - Can be exported to clipboard or restored at any time.
            """.ReplaceLineEndings("\n").Loc());
        var groups = P.Archive.LayoutsL.Select(x => x.Group).Distinct().Order();

        foreach(var group in groups)
        {
            if(group == "") continue;
            if(ImGuiEx.TreeNode(group))
            {
                var grp = P.Archive.LayoutsL.Where(x => x.Group == group);
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Copy, "Copy group".Loc()))
                {
                    Copy(grp.Select(x => EzConfig.DefaultSerializationFactory.Serialize(x, false)).Join("\n"));
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.ArrowCircleLeft, "Restore group".Loc()))
                {
                    foreach(var x in grp)
                    {
                        P.Config.LayoutsL.Add(x.JSONClone());
                        new TickScheduler(() => P.Archive.LayoutsL.Remove(x));
                    }
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Trash, "Delete group".Loc(), ImGuiEx.Ctrl))
                {
                    foreach(var x in grp)
                    {
                        new TickScheduler(() => P.Archive.LayoutsL.Remove(x));
                    }
                }
                ImGui.PushID(group);
                DrawArchiveEntries(grp);
                ImGui.PopID();
                ImGui.TreePop();
            }
        }
        var nogrp = P.Archive.LayoutsL.Where(x => x.Group == "");
        if(nogrp.Any()) DrawArchiveEntries(nogrp);
    }

    private void DrawArchiveEntries(IEnumerable<Layout> layouts)
    {
        if(ImGui.BeginTable("EntryArchive", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("Name".Loc(), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Info".Loc());
            ImGui.TableSetupColumn("Control".Loc());

            foreach(var x in layouts)
            {
                ImGui.PushID(x.GUID);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGuiEx.TextV($"{x.Name}");

                ImGui.TableNextColumn();

                ImGuiEx.TextV("?? elements".Loc(x.ElementsL.Count));

                ImGui.TableNextColumn();

                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Copy, "Copy".Loc()))
                {
                    Copy(EzConfig.DefaultSerializationFactory.Serialize(x, false));
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.ArrowCircleLeft, "Restore".Loc()))
                {
                    P.Config.LayoutsL.Add(x.JSONClone());
                    new TickScheduler(() => P.Archive.LayoutsL.Remove(x));
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Trash, "Delete".Loc(), ImGuiEx.Ctrl))
                {
                    new TickScheduler(() => P.Archive.LayoutsL.Remove(x));
                }
                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }
}
