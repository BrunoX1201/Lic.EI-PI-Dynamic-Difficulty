from typing import Any


def numpy_to_native(value: Any) -> Any:
    if hasattr(value, "item"):
        return value.item()
    return value
