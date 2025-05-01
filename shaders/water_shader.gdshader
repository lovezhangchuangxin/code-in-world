shader_type canvas_item;

// 调节水的颜色
uniform vec4 tint : source_color = vec4(0.2, 0.6, 1.0, 0.7);

// 波浪参数
uniform float wave_speed = 2.0;
uniform float wave_amplitude = 0.02;
uniform float wave_frequency = 10.0;
uniform vec2 wave_direction = vec2(1.0, 1.0);

// 扭曲和变形参数
uniform sampler2D noise_texture;
uniform float distortion_strength = 0.02;
uniform float time_scale = 1.0;

// 波纹效果
uniform float ripple_strength = 0.005;

void fragment() {
    // 基本 UV 坐标
    vec2 uv = UV;

    // 计算波浪效果 - 使用正弦波创造水面波动
    float wave_offset = sin(uv.x * wave_frequency + TIME * wave_speed) *
                        cos(uv.y * wave_frequency * 0.8 + TIME * wave_speed * 0.8) *
                        wave_amplitude;

    // 应用波浪偏移
    uv += normalize(wave_direction) * wave_offset;

    // 从噪声纹理获取扭曲值，使用TIME创建流动感
    vec2 noise_uv = uv + TIME * time_scale * 0.1;
    vec2 noise_value = texture(noise_texture, noise_uv).rg * 2.0 - 1.0;

    // 应用扭曲
    uv += noise_value * distortion_strength;

    // 创建波纹效果
    float ripple = sin(length(uv * 10.0) - TIME * 2.0) * ripple_strength;
    uv += ripple;

    // 采样原始纹理（使用扭曲后的UV）
    vec4 color = texture(TEXTURE, uv);

    // 混合水的颜色
    color = mix(color, tint, tint.a * 0.7);

    // 添加一些亮度变化，模拟水面反光
    float brightness = sin(TIME * 0.5) * 0.05 + 0.05;
    color.rgb += brightness;

    COLOR = color;
}