shader_type canvas_item;

// 基本参数
uniform vec4 base_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);  // 基础颜色调整
uniform vec2 light_origin = vec2(0.0, -1.0);  // 光源方向，默认从上方照射
uniform float shadow_strength = 0.4;  // 阴影强度
uniform float highlight_strength = 0.25;  // 高光强度
uniform float ambient_light = 0.6;  // 环境光强度，防止阴影过暗

void fragment() {
    // 获取原始纹理颜色
    vec4 color = texture(TEXTURE, UV);
    
    // 计算以中心为基准的UV坐标
    vec2 centered_uv = UV - vec2(0.5, 0.5);
    
    // 计算从中心到当前像素的方向
    vec2 direction_from_center = normalize(centered_uv);
    
    // 计算光照强度 - 基于光源方向和到中心的方向的点积
    float light_intensity = max(0.0, dot(-direction_from_center, normalize(light_origin)));
    light_intensity = ambient_light + (1.0 - ambient_light) * light_intensity;
    
    // 根据到中心的距离添加一些变化，使边缘更暗
    float edge_shadow = 1.0 - length(centered_uv) * shadow_strength;
    edge_shadow = max(edge_shadow, 1.0 - shadow_strength);  // 限制最小值
    
    // 计算高光 - 靠近光源方向的区域
    float highlight = pow(light_intensity, 3.0) * highlight_strength;
    
    // 应用光照效果
    vec3 lit_color = color.rgb * base_color.rgb * light_intensity * edge_shadow;
    lit_color += highlight * color.a;  // 只在不透明区域添加高光
    
    // 输出最终颜色，保留原始alpha值
    COLOR = vec4(lit_color, color.a);
}