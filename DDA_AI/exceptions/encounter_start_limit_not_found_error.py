class EncounterStartLimitNotFound(Exception):

    def __init__(self, msg):
        self.message = msg
        super().__init__(self.message)
