from .min_max_applicable_additions import min_max_applicable_additions


def calculate_additive_offset(min_limit: int | float, max_limit: int | float, default_value: int | float,
                              current_value: int | float,
                              step_percentage: float,
                              n_decimals: int, force_step: bool = True) -> float:
    max_default_substraction, max_default_addition = min_max_applicable_additions(min_limit, max_limit, default_value,
                                                                                  n_decimals)
    max_current_substraction, max_current_addition = min_max_applicable_additions(min_limit, max_limit, current_value,
                                                                                  n_decimals)

    step = default_value * (step_percentage - 1)
    step = max(max_default_substraction, min(step, max_default_addition))  # clamp (min_default, step, max_default)
    intended_direction = 1 if step > 0 else -1
    will_lose_step = round(step, n_decimals) == 0

    curr_offset = 0
    if current_value < min_limit or current_value > max_limit:
        # POSSIBLE IMPROVEMENT, transform the interval (min-limit, max-limit) to correspond with the current value
        # maybe save the step of the last encounter and add it here to continue the difficulty
        curr_offset = default_value - current_value

    final_step = float(step + curr_offset)
    final_step = max(max_current_substraction, min(round(final_step, n_decimals), max_current_addition))

    if force_step:
        if not will_lose_step:
            return final_step

        forced_step = pow(10, -n_decimals) * intended_direction
        final_forced_step = float(forced_step + curr_offset)
        return max(max_current_substraction, min(round(final_forced_step, n_decimals), max_current_addition))

    return final_step
