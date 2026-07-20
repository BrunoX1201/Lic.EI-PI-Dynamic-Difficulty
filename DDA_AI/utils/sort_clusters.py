from collections import OrderedDict

import numpy as np


def sort_clusters(clusters: np.ndarray[tuple[float, float], np.dtype[np.float64]],
                  feature_weights: list[float],
                  feature_comparisons: list[str]) -> list[int]:
    """
    :param feature_weights: values between 0 and 1 (inclusive)
    :param feature_comparisons: possible values "max" or "min"
    :return: a sorted rank (descending order) where each value represents the index of the cluster in clusters
    """
    n_clusters, n_features = clusters.shape
    n_weights = len(feature_weights)
    n_comparisons = len(feature_comparisons)

    if n_weights != n_features or n_comparisons != n_features:
        raise ValueError("Feature weights or comparisons do not match cluster features")

    total_differences_per_cluster = {i: 0 for i in range(n_clusters)}

    normalized_clusters = np.zeros((n_clusters, n_features))

    for i in range(n_features):
        feature_col = clusters[:, i]
        normalized_feature = (feature_col - np.min(feature_col)) / (np.max(feature_col) - np.min(feature_col))
        normalized_clusters[:, i] = normalized_feature

    for i in range(n_features):
        cp_index = np.argmax(normalized_clusters[:, i]) if feature_comparisons[i] == "max" else np.argmin(
            normalized_clusters[:, i])
        for j in range(n_clusters):
            total_differences_per_cluster[j] += abs(normalized_clusters[j, i] - normalized_clusters[cp_index, i]) * \
                                                feature_weights[i]

    ordered_clusters = OrderedDict(sorted(total_differences_per_cluster.items(), key=lambda item: item[1]))
    return list(ordered_clusters.keys())
