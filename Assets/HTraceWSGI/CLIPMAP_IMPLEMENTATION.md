# HTraceWSGI Clipmap 实现说明

## 概述

本次更新为HTraceWSGI体素化GI系统添加了**Clipmap支持**，解决了光线追踪范围受限的问题。

### 问题描述
- **原系统限制**：体素化范围最大为80米（默认60米），256x256x256纹理分辨率
- **问题**：超出60米范围的光线无法trace到体素，导致远距离GI失效

### 解决方案
- **新增Clipmap层级**：第二级体素覆盖范围可达150米，使用相同的256x256x256纹理
- **体素尺寸**：Clipmap使用2倍基础体素大小（更低分辨率，覆盖更大范围）
- **无缝追踪**：光线先在基础体素中追踪，未命中时自动切换到Clipmap层级

## 主要修改文件

### 1. C# 代码修改

#### `Scripts/HConfig.cs`
- 已存在 `MAX_VOXEL_BOUNDS_CLIPMAP = 150` 配置

#### `Scripts/Data/Public/VoxelizationSettings.cs`
- 已存在 `EnableClipmaps` 开关
- 已存在 `ClipmapRange` 参数（范围：80-150米）

#### `Scripts/Data/Private/VoxelizationExactData.cs`
- **新增**：Clipmap分辨率、边界、体素大小等参数计算
- Clipmap使用2倍体素大小以覆盖更大范围

#### `Scripts/Passes/Shared/VoxelizationShared.cs`
- **新增**：Clipmap纹理的shader参数ID
- **新增**：Clipmap RTWrapper对象声明

#### `Scripts/Passes/HDRP/VoxelizationPassHDRP.cs`
- **新增**：Clipmap纹理分配逻辑（VoxelClipmapData、VoxelClipmapPositionPyramid等）
- **新增**：Clipmap纹理清除和绑定
- **新增**：Clipmap位置金字塔生成
- **新增**：Clipmap纹理作为RandomWriteTarget绑定到体素化shader
- **更新**：重新分配条件检查（EnableClipmaps状态变化时重新分配）

### 2. HLSL Shader 修改

#### `Resources/HTraceWSGI/Includes/VoxelizationCommon.hlsl`
- 已存在Clipmap参数声明
- 已存在Clipmap纹理声明

#### `Resources/HTraceWSGI/Includes/VoxelizationStages.hlsl`
- **新增**：Fragment shader中添加Clipmap写入逻辑
- 检查几何体是否在Clipmap范围内（基础范围外，Clipmap范围内）
- 使用相同的体素打包格式写入Clipmap纹理

#### `Resources/HTraceWSGI/Includes/VoxelTraversal.hlsl`
- **新增**：`TraceVoxelsDiffuse`函数中添加Clipmap追踪逻辑
- 光线首先在基础体素中追踪
- 如果未命中且启用了Clipmap，继续在Clipmap中追踪
- Clipmap追踪从基础体素边界开始，使用更大的步进尺寸

#### `Resources/HTraceWSGI/Includes/VoxelLightingEvaluation.hlsl`
- **新增**：`EvaluateHitLighting`函数中添加Clipmap数据读取
- 根据HitCoord判断命中点来自基础体素还是Clipmap
- 从正确的纹理读取体素颜色数据

## 使用方法

### 启用Clipmap功能

1. 在Unity编辑器中找到HTraceWSGI组件
2. 展开 **Voxelization Settings**
3. 勾选 **Enable Clipmaps** 选项
4. 设置 **Clipmap Range**（建议值：120-150米）

### 参数说明

| 参数 | 说明 | 推荐值 |
|------|------|--------|
| **Voxel Bounds** | 基础体素范围 | 60-80米 |
| **Enable Clipmaps** | 启用Clipmap | True |
| **Clipmap Range** | Clipmap覆盖范围 | 120-150米 |
| **Voxel Density** | 体素密度 | 0.64 |

### 性能考虑

- **内存占用**：Clipmap增加一个256³的3D纹理（~64MB for R32_UInt）
- **渲染开销**：体素化pass需要额外写入Clipmap纹理
- **追踪性能**：光线追踪在未命中基础体素时会继续追踪Clipmap（最多增加50%迭代次数）

### 最佳实践

1. **场景适配**：
   - 开阔场景（户外）：启用Clipmap，设置150米范围
   - 室内场景：可以禁用Clipmap，节省内存

2. **范围设置**：
   - 基础体素：覆盖相机周围精细细节（60米）
   - Clipmap：覆盖远距离粗略GI（150米）

3. **质量平衡**：
   - Clipmap使用2倍体素大小，精度较低
   - 适合远距离间接光照，不适合近距离细节

## 技术细节

### Clipmap体素尺寸
```
基础体素大小 = bounds / resolution
Clipmap体素大小 = 基础体素大小 × 2
```

### 光线追踪流程
1. 从射线原点开始在基础体素中追踪
2. 使用HDDA算法（分层DDA）进行加速
3. 如果在基础范围内未命中：
   - 检查是否启用Clipmap
   - 从基础边界开始在Clipmap中继续追踪
   - 使用相同的HDDA算法但步进更大

### 数据结构
两个层级共享相同的数据打包格式：
```
32位整数存储：
- [31-24]: 八叉树占位标记（8位）
- [23]: 静态/动态标记
- [22-0]: 压缩颜色和发光标记
```

## 调试建议

1. **可视化体素**：
   - 使用Debug模式查看体素化结果
   - 确认Clipmap区域是否正确渲染

2. **检查范围**：
   - 确保Clipmap Range > Voxel Bounds
   - 建议Clipmap Range至少是Voxel Bounds的1.5倍

3. **性能监控**：
   - 使用Unity Profiler查看体素化和追踪耗时
   - 如果性能不足，可以减小Clipmap Range或禁用功能

## 限制和已知问题

1. **当前仅支持Constant Voxelization模式**
   - Partial Voxelization模式的Clipmap支持待后续添加

2. **Clipmap不支持滚动更新**
   - 每帧完全重新体素化Clipmap区域

3. **精度限制**
   - Clipmap使用2倍体素大小，远距离细节较少
   - 适合间接光照，不适合精确阴影

## 版本信息

- **实现日期**：2026-03-05
- **修改文件数**：7个文件
- **新增代码行数**：约500行
- **兼容版本**：Unity HDRP 2022.3+

## 贡献者

本功能由AI助手实现，基于用户需求优化HTraceWSGI的光线追踪范围。
