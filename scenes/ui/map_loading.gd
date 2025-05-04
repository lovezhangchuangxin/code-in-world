extends CanvasLayer

@onready var control: Control = $Control
@onready var label: Label = $Control/Label

@export
var dot_num := 6

var base_text: String
var current_dot_num := 0

func _ready() -> void:
    base_text = label.text if label.text else "地图加载中"

func _on_map_loader_map_init() -> void:
    var tween := create_tween()
    tween.tween_property(control, "modulate", Color(0, 0, 0, 0), 0.7)
    tween.tween_callback(queue_free).set_delay(0.7)

func _on_timer_timeout() -> void:
    current_dot_num = (current_dot_num + 1) % (dot_num + 1)
    label.text = base_text + ".".repeat(current_dot_num)