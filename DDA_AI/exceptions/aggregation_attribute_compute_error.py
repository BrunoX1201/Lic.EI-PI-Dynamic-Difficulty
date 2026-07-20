class AggregationAttributeComputeError(Exception):

    def __init__(self, attr_name: str) -> None:
        self.message = f"Could not compute {attr_name} attribute!"
        super().__init__(self.message)
