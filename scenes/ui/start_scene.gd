extends CanvasLayer

@onready var start_button: Button = $Control/VBoxContainer/StartButton
@onready var new_game_button: Button = $Control/VBoxContainer/NewGameButton
@onready var load_game_button: Button = $Control/VBoxContainer/LoadGameButton
@onready var option_button: Button = $Control/VBoxContainer/OptionButton
@onready var exit_button: Button = $Control/VBoxContainer/ExitButton

func _on_start_button_pressed() -> void:
    SceneManager.load_scene(SceneConstant.SCENE_PATH["Main"])

func _on_load_game_button_pressed() -> void:
    pass # Replace with function body.

func _on_new_game_button_pressed() -> void:
    pass # Replace with function body.

func _on_option_button_pressed() -> void:
    pass # Replace with function body.

func _on_exit_button_pressed() -> void:
    # TODO: 退出前记得保存
    get_tree().quit()
