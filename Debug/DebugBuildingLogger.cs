using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Ts_Core.Models;
using Ts_Core.Services.BuildingRelated;

namespace Ts_Core.Debug
{
    /// <summary>
    /// Building関連のデバッグ情報を表示します。
    /// </summary>
    internal static class DebugBuildingLogger
    {
        //----------------------------------------
        // Farm Buildings
        //----------------------------------------

        /// <summary>
        /// 農場に存在する建物情報を表示します。
        /// </summary>
        internal static void LogFarmBuildings(
            IMonitor monitor)
        {
            if (!Context.IsWorldReady)
            {
                monitor.Log(
                    "No save is loaded.",
                    LogLevel.Warn);

                return;
            }

            Farm farm =
                Game1.getFarm();

            monitor.Log(
                "===== Farm Buildings =====",
                LogLevel.Info);

            monitor.Log(
                $"Registered Buildings: {farm.buildings.Count}",
                LogLevel.Info);

            foreach (Building building in farm.buildings)
            {
                DebugLogHelper.LogBlankLine(
                    monitor);

                monitor.Log(
                    building.buildingType.Value,
                    LogLevel.Info);

                DebugLogHelper.LogField(
                    monitor,
                    "Tile",
                    $"({building.tileX.Value}, {building.tileY.Value})");

                DebugLogHelper.LogField(
                    monitor,
                    "Size",
                    $"{building.tilesWide.Value} x {building.tilesHigh.Value}");

                DebugLogHelper.LogField(
                    monitor,
                    "Indoors",
                    building.GetIndoors()?.NameOrUniqueName
                        ?? "(none)");
            }
        }

        //----------------------------------------
        // Building Providers
        //----------------------------------------

        /// <summary>
        /// 現在登録されているBuilding Providerを表示します。
        /// Provider IDが指定された場合は詳細を表示します。
        /// </summary>
        internal static void LogBuildingProviders(
            IMonitor monitor,
            string[] args)
        {
            IReadOnlyList<RegisteredBuildingProviderInfo> providers =
                BuildingProviderService.GetRegisteredProviders();

            //----------------------------------------
            // ID指定あり
            //----------------------------------------

            if (args.Length > 0
                && !string.IsNullOrWhiteSpace(args[0]))
            {
                string providerId =
                    args[0].Trim();

                RegisteredBuildingProviderInfo? provider =
                    providers.FirstOrDefault(
                        entry =>
                            string.Equals(
                                entry.Id,
                                providerId,
                                StringComparison.OrdinalIgnoreCase));

                if (provider == null)
                {
                    monitor.Log(
                        $"Building Provider '{providerId}' was not found.",
                        LogLevel.Warn);

                    return;
                }

                LogBuildingProviderDetails(
                    monitor,
                    provider);

                return;
            }

            //----------------------------------------
            // 一覧表示
            //----------------------------------------

            monitor.Log(
                "===== Building Providers =====",
                LogLevel.Info);

            monitor.Log(
                $"Registered Providers: {providers.Count}",
                LogLevel.Info);

            if (providers.Count == 0)
            {
                DebugLogHelper.LogBlankLine(
                    monitor);

                monitor.Log(
                    "No Building Providers are registered.",
                    LogLevel.Info);

                return;
            }

            DebugLogHelper.LogBlankLine(
                monitor);

            monitor.Log(
                "To view provider details, enter its ID.",
                LogLevel.Info);

            monitor.Log(
                "Example: tscore_debug_buildings MyBuildingID",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            //----------------------------------------
            // 登録元ごとにグループ化
            //----------------------------------------

            List<IGrouping<string, RegisteredBuildingProviderInfo>> groupList =
                providers
                    .GroupBy(provider => provider.Owner)
                    .OrderBy(group =>
                        string.Equals(
                            group.Key,
                            "T's Core",
                            StringComparison.OrdinalIgnoreCase)
                            ? 0
                            : 1)
                    .ThenBy(group => group.Key)
                    .ToList();

            DebugLogHelper.LogBlankLine(
                monitor);

            for (int groupIndex = 0;
                 groupIndex < groupList.Count;
                 groupIndex++)
            {
                IGrouping<string, RegisteredBuildingProviderInfo> group =
                    groupList[groupIndex];

                List<RegisteredBuildingProviderInfo> providerList =
                    group
                        .OrderBy(provider => provider.Id)
                        .ToList();

                monitor.Log(
                    $"----- {group.Key} ({providerList.Count}) -----",
                    LogLevel.Info);

                DebugLogHelper.LogBlankLine(
                    monitor);

                foreach (RegisteredBuildingProviderInfo provider in providerList)
                {
                    monitor.Log(
                        $"    {provider.Id}",
                        LogLevel.Info);
                }

                if (groupIndex < groupList.Count - 1)
                {
                    DebugLogHelper.LogBlankLine(
                        monitor);
                }
            }
        }

        //----------------------------------------
        // Building Provider 詳細
        //----------------------------------------

        /// <summary>
        /// Building Provider一件分の詳細情報を表示します。
        /// </summary>
        private static void LogBuildingProviderDetails(
            IMonitor monitor,
            RegisteredBuildingProviderInfo provider)
        {
            monitor.Log(
                "===== Building Provider =====",
                LogLevel.Info);

            DebugLogHelper.LogBlankLine(
                monitor);

            monitor.Log(
                provider.Id,
                LogLevel.Info);

            DebugLogHelper.LogField(
                monitor,
                "Owner",
                provider.Owner);

            DebugLogHelper.LogField(
                monitor,
                "Source File",
                provider.SourceFile);

            DebugLogHelper.LogField(
                monitor,
                "Building",
                provider.BuildingType);

            DebugLogHelper.LogField(
                monitor,
                "Enable Field",
                string.IsNullOrWhiteSpace(
                    provider.LightsEnabledField)
                    ? "(none)"
                    : provider.LightsEnabledField);

            DebugLogHelper.LogField(
                monitor,
                "Lights",
                provider.Lights.Count);

            DebugLogHelper.LogField(
                monitor,
                "DrawLayers",
                provider.DrawLayers.Count);

            //----------------------------------------
            // Lights
            //----------------------------------------

            if (provider.Lights.Count > 0)
            {
                DebugLogHelper.LogBlankLine(
                    monitor);

                monitor.Log(
                    "----- Lights -----",
                    LogLevel.Info);

                DebugLogHelper.LogBlankLine(
                    monitor);

                for (int i = 0;
                     i < provider.Lights.Count;
                     i++)
                {
                    BuildingLightModel light =
                        provider.Lights[i];

                    monitor.Log(
                        $"    {light.Id}",
                        LogLevel.Info);

                    DebugLogHelper.LogField(
                        monitor,
                        "Offset",
                        $"({light.OffsetX}, {light.OffsetY})",
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Radius",
                        light.Radius,
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Color",
                        light.Color,
                        indent: 8);

                    if (i < provider.Lights.Count - 1)
                    {
                        DebugLogHelper.LogBlankLine(
                            monitor);
                    }
                }
            }

            //----------------------------------------
            // DrawLayers
            //----------------------------------------

            if (provider.DrawLayers.Count > 0)
            {
                DebugLogHelper.LogBlankLine(
                    monitor);

                monitor.Log(
                    "----- DrawLayers -----",
                    LogLevel.Info);

                DebugLogHelper.LogBlankLine(
                    monitor);

                for (int i = 0;
                     i < provider.DrawLayers.Count;
                     i++)
                {
                    BuildingDrawLayerModel layer =
                        provider.DrawLayers[i];

                    monitor.Log(
                        $"    {layer.Id}",
                        LogLevel.Info);

                    DebugLogHelper.LogField(
                        monitor,
                        "Texture",
                        string.IsNullOrWhiteSpace(layer.Texture)
                            ? "(Building Texture)"
                            : layer.Texture,
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "SourceRect",
                        $"({layer.SourceRect.X}, {layer.SourceRect.Y}, " +
                        $"{layer.SourceRect.Width}, {layer.SourceRect.Height})",
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "DrawPosition",
                        $"({layer.DrawPosition.X}, {layer.DrawPosition.Y})",
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Background",
                        layer.DrawInBackground,
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Sort Offset",
                        layer.SortTileOffset,
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Chest",
                        string.IsNullOrWhiteSpace(
                            layer.OnlyDrawIfChestHasContents)
                            ? "(none)"
                            : layer.OnlyDrawIfChestHasContents,
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Frame Duration",
                        layer.FrameDuration,
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Frame Count",
                        layer.FrameCount,
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Frames Per Row",
                        layer.FramesPerRow,
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Animal Door Offset",
                        $"({layer.AnimalDoorOffset.X}, {layer.AnimalDoorOffset.Y})",
                        indent: 8);

                    DebugLogHelper.LogField(
                        monitor,
                        "Condition",
                        string.IsNullOrWhiteSpace(layer.Condition)
                            ? "(none)"
                            : layer.Condition,
                        indent: 8);

                    if (i < provider.DrawLayers.Count - 1)
                    {
                        DebugLogHelper.LogBlankLine(
                            monitor);
                    }
                }
            }
        }

        //----------------------------------------
        // Building Light
        //----------------------------------------

        // 開発用メモ:
        // ・内部確認用コマンド
        // ・Modder Guide等には記載しない
        // ・後々削除予定

        /// <summary>
        /// 現在登録されているBuilding Light情報を表示します。
        /// </summary>
        internal static void LogBuildingLights(
            IMonitor monitor)
        {
            IReadOnlyList<RegisteredBuildingProviderInfo> providers =
                BuildingProviderService.GetRegisteredProviders();

            int lightCount =
                providers.Sum(
                    provider => provider.Lights.Count);

            monitor.Log(
                "===== Building Lights =====",
                LogLevel.Info);

            monitor.Log(
                $"Registered Lights: {lightCount}",
                LogLevel.Info);

            if (lightCount == 0)
            {
                DebugLogHelper.LogBlankLine(
                    monitor);

                monitor.Log(
                    "No Building Lights are registered.",
                    LogLevel.Info);

                return;
            }

            List<IGrouping<string, RegisteredBuildingProviderInfo>> groupList =
                providers
                    .Where(provider =>
                        provider.Lights.Count > 0)
                    .GroupBy(provider => provider.Owner)
                    .OrderBy(group =>
                        string.Equals(
                            group.Key,
                            "T's Core",
                            StringComparison.OrdinalIgnoreCase)
                            ? 0
                            : 1)
                    .ThenBy(group => group.Key)
                    .ToList();

            DebugLogHelper.LogBlankLine(
                monitor);

            for (int groupIndex = 0;
                 groupIndex < groupList.Count;
                 groupIndex++)
            {
                IGrouping<string, RegisteredBuildingProviderInfo> group =
                    groupList[groupIndex];

                monitor.Log(
                    $"----- {group.Key} -----",
                    LogLevel.Info);

                DebugLogHelper.LogBlankLine(
                    monitor);

                List<RegisteredBuildingProviderInfo> providerList =
                    group
                        .OrderBy(provider => provider.Id)
                        .ToList();

                for (int providerIndex = 0;
                     providerIndex < providerList.Count;
                     providerIndex++)
                {
                    RegisteredBuildingProviderInfo provider =
                        providerList[providerIndex];

                    monitor.Log(
                        provider.Id,
                        LogLevel.Info);

                    DebugLogHelper.LogField(
                        monitor,
                        "Building",
                        provider.BuildingType);

                    DebugLogHelper.LogField(
                        monitor,
                        "Lights",
                        provider.Lights.Count);

                    DebugLogHelper.LogField(
                        monitor,
                        "Enable Field",
                        string.IsNullOrWhiteSpace(
                            provider.LightsEnabledField)
                            ? "(none)"
                            : provider.LightsEnabledField);

                    if (provider.Lights.Count > 0)
                    {
                        DebugLogHelper.LogBlankLine(
                            monitor);

                        for (int lightIndex = 0;
                             lightIndex < provider.Lights.Count;
                             lightIndex++)
                        {
                            BuildingLightModel light =
                                provider.Lights[lightIndex];

                            monitor.Log(
                                $"    {light.Id}",
                                LogLevel.Info);

                            DebugLogHelper.LogField(
                                monitor,
                                "Offset",
                                $"({light.OffsetX}, {light.OffsetY})",
                                indent: 8);

                            DebugLogHelper.LogField(
                                monitor,
                                "Radius",
                                light.Radius,
                                indent: 8);

                            DebugLogHelper.LogField(
                                monitor,
                                "Color",
                                light.Color,
                                indent: 8);

                            if (lightIndex < provider.Lights.Count - 1)
                            {
                                DebugLogHelper.LogBlankLine(
                                    monitor);
                            }
                        }
                    }

                    if (providerIndex < providerList.Count - 1)
                    {
                        DebugLogHelper.LogBlankLine(
                            monitor);
                    }
                }

                if (groupIndex < groupList.Count - 1)
                {
                    DebugLogHelper.LogBlankLine(
                        monitor);
                }
            }
        }

        //----------------------------------------
        // Building DrawLayer
        //----------------------------------------

        // 開発用メモ:
        // ・内部確認用コマンド
        // ・Modder Guide等には記載しない
        // ・後々削除予定

        /// <summary>
        /// 現在登録されている条件付きBuilding DrawLayerを表示します。
        /// </summary>
        internal static void LogBuildingDrawLayers(
            IMonitor monitor)
        {
            IReadOnlyList<RegisteredBuildingProviderInfo> providers =
                BuildingProviderService.GetRegisteredProviders();

            int drawLayerCount =
                providers.Sum(
                    provider => provider.DrawLayers.Count);

            monitor.Log(
                "===== Building DrawLayers =====",
                LogLevel.Info);

            monitor.Log(
                $"Registered DrawLayers: {drawLayerCount}",
                LogLevel.Info);

            if (drawLayerCount == 0)
            {
                DebugLogHelper.LogBlankLine(
                    monitor);

                monitor.Log(
                    "No conditional Building DrawLayers are registered.",
                    LogLevel.Info);

                return;
            }

            List<IGrouping<string, RegisteredBuildingProviderInfo>> groupList =
                providers
                    .Where(provider =>
                        provider.DrawLayers.Count > 0)
                    .GroupBy(provider => provider.Owner)
                    .OrderBy(group =>
                        string.Equals(
                            group.Key,
                            "T's Core",
                            StringComparison.OrdinalIgnoreCase)
                            ? 0
                            : 1)
                    .ThenBy(group => group.Key)
                    .ToList();

            DebugLogHelper.LogBlankLine(
                monitor);

            for (int groupIndex = 0;
                 groupIndex < groupList.Count;
                 groupIndex++)
            {
                IGrouping<string, RegisteredBuildingProviderInfo> group =
                    groupList[groupIndex];

                monitor.Log(
                    $"----- {group.Key} -----",
                    LogLevel.Info);

                DebugLogHelper.LogBlankLine(
                    monitor);

                List<RegisteredBuildingProviderInfo> providerList =
                    group
                        .OrderBy(provider => provider.Id)
                        .ToList();

                for (int providerIndex = 0;
                     providerIndex < providerList.Count;
                     providerIndex++)
                {
                    RegisteredBuildingProviderInfo provider =
                        providerList[providerIndex];

                    monitor.Log(
                        provider.Id,
                        LogLevel.Info);

                    DebugLogHelper.LogField(
                        monitor,
                        "Building",
                        provider.BuildingType);

                    DebugLogHelper.LogField(
                        monitor,
                        "DrawLayers",
                        provider.DrawLayers.Count);

                    DebugLogHelper.LogBlankLine(
                        monitor);

                    for (int layerIndex = 0;
                         layerIndex < provider.DrawLayers.Count;
                         layerIndex++)
                    {
                        BuildingDrawLayerModel layer =
                            provider.DrawLayers[layerIndex];

                        monitor.Log(
                            $"    {layer.Id}",
                            LogLevel.Info);

                        DebugLogHelper.LogField(
                            monitor,
                            "Texture",
                            string.IsNullOrWhiteSpace(layer.Texture)
                                ? "(Building Texture)"
                                : layer.Texture,
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "SourceRect",
                            $"({layer.SourceRect.X}, {layer.SourceRect.Y}, " +
                            $"{layer.SourceRect.Width}, {layer.SourceRect.Height})",
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "DrawPosition",
                            $"({layer.DrawPosition.X}, {layer.DrawPosition.Y})",
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "Background",
                            layer.DrawInBackground,
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "Sort Offset",
                            layer.SortTileOffset,
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "Frames",
                            layer.FrameCount,
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "Frame Duration",
                            layer.FrameDuration,
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "Frames Per Row",
                            layer.FramesPerRow,
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "Chest",
                            string.IsNullOrWhiteSpace(
                                layer.OnlyDrawIfChestHasContents)
                                ? "(none)"
                                : layer.OnlyDrawIfChestHasContents,
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "Animal Door Offset",
                            $"({layer.AnimalDoorOffset.X}, {layer.AnimalDoorOffset.Y})",
                            indent: 8);

                        DebugLogHelper.LogField(
                            monitor,
                            "Condition",
                            string.IsNullOrWhiteSpace(layer.Condition)
                                ? "(none)"
                                : layer.Condition,
                            indent: 8);

                        if (layerIndex < provider.DrawLayers.Count - 1)
                        {
                            DebugLogHelper.LogBlankLine(
                                monitor);
                        }
                    }

                    if (providerIndex < providerList.Count - 1)
                    {
                        DebugLogHelper.LogBlankLine(
                            monitor);
                    }
                }

                if (groupIndex < groupList.Count - 1)
                {
                    DebugLogHelper.LogBlankLine(
                        monitor);
                }
            }
        }
    }
}