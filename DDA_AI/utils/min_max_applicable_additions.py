def min_max_applicable_additions(min_limit: int | float, max_limit: int | float, current_value: int | float,
                                 n_decimals: int) -> tuple[
    float, float]:
    """
    :param min_limit:
    :param max_limit:
    :param current_value:
    :param n_decimals:
    :return: tuple of (max_negative_addition, max_positive_addition)
    """
    max_negative_addition = min_limit - current_value
    max_positive_addition = max_limit - current_value

    rounded_max_negative_addition = round(max_negative_addition, n_decimals)
    trunc = '%.' + str(n_decimals) + 'f'
    truncated_max_positive_addition = float(trunc % max_positive_addition)

    if rounded_max_negative_addition > truncated_max_positive_addition:
        raise ValueError(
            f"No valid additions exists after rounding/truncation: max_negative={rounded_max_negative_addition} ({max_negative_addition}, max_positive={truncated_max_positive_addition} ({max_positive_addition})")

    return rounded_max_negative_addition, truncated_max_positive_addition
