import uuid
import random

def random_uuid_string(length=16):
    key = str(uuid.uuid4()).replace("-", "")
    while len(key) < length:
        key = key + str(uuid.uuid4()).replace("-", "")
    return key[:length]

VALID_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+{}:;-=,./<>?"
def random_string(length=16):
    return ''.join(random.choice(VALID_CHARS) for _ in range(length))