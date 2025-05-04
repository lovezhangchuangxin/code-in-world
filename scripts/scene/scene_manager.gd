# ------------------------------------
#            场景管理器
#        负责场景的加载和切换
# ------------------------------------
extends Node

# 加载新的场景
func load_scene(scene_path: String) -> void:
    get_tree().change_scene_to_file(scene_path)