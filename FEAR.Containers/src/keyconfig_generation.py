import base64
import hashlib
import uuid
from datetime import datetime, timezone

from Crypto.Cipher import AES
from Crypto.Util.Padding import pad


def calculate_key_encryption_iv_static(key_name, key_identifiers):
    ordered_keys = key_identifiers

    all_key_values = "".join(ordered_keys.values())
    key_value = all_key_values

    while len(key_value) < 32:
        key_value = key_value + key_value

    key_bytes = key_value[:32].encode("utf-8")
    iv_bytes = hashlib.md5(all_key_values.encode("utf-8")).digest()[:16]

    return key_bytes, iv_bytes


def encrypt_secret_with_keys(secret, key_name, key_identifiers):
    key_bytes, iv_bytes = calculate_key_encryption_iv_static(key_name, key_identifiers)

    cipher = AES.new(key_bytes, AES.MODE_CBC, iv_bytes)
    encrypted = cipher.encrypt(pad(secret.encode("utf-8"), AES.block_size))

    return base64.b64encode(encrypted).decode("utf-8")


def generate_key_set():
    secret = str(uuid.uuid4())[:16]
    key_name = f"DefaultKey-{datetime.now(timezone.utc):%Y%m%d}"

    primary_key = str(uuid.uuid4())[:8]
    secondary_key = str(uuid.uuid4())[:8]

    return {
        "KeyName": key_name,
        "KeyIdentifiers": ["PrimaryKey", "SecondaryKey"],
        "KeySecrets": {
            "PrimaryKey": primary_key,
            "SecondaryKey": secondary_key,
        },
        "EncryptedKey": encrypt_secret_with_keys(
            secret,
            key_name,
            {
                "PrimaryKey": primary_key,
                "SecondaryKey": secondary_key,
            },
        ),
    }

