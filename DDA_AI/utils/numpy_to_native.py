def numpy_to_native(value):
    if hasattr(value, "item"):
        return value.item()
    return value
