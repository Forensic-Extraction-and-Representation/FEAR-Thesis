from cryptography import x509
from cryptography.x509.oid import NameOID
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import rsa
from datetime import datetime, timedelta
import os
import base64

def generate_root_ca(country, state, locality, ca_file_private, ca_file_cert, ca_password):
    # Generate private key
    private_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    # Create self-signed certificate
    print("Generating self-signed certificate...")
    subject = issuer = x509.Name([
        x509.NameAttribute(NameOID.COUNTRY_NAME, country),
        x509.NameAttribute(NameOID.STATE_OR_PROVINCE_NAME, state),
        x509.NameAttribute(NameOID.LOCALITY_NAME, locality),
        x509.NameAttribute(NameOID.ORGANIZATION_NAME, u"FEAR Application"),
        x509.NameAttribute(NameOID.COMMON_NAME, u"FEAR Root CA"),
    ])
    
    certificate = x509.CertificateBuilder().subject_name(subject) \
        .issuer_name(issuer) \
        .public_key(private_key.public_key()) \
        .serial_number(x509.random_serial_number()) \
        .not_valid_before(datetime.utcnow()) \
        .not_valid_after(datetime.utcnow() + timedelta(days=18200)) \
        .add_extension(x509.BasicConstraints(ca=True, path_length=None), critical=True) \
        .sign(private_key, hashes.SHA256())
    # Save private key and certificate to files
    print(f"Saving CA private key to {ca_file_private} and certificate to {ca_file_cert}...")

    with open(ca_file_private, "wb") as f:
        f.write(private_key.private_bytes(encoding=serialization.Encoding.PEM, format=serialization.PrivateFormat.TraditionalOpenSSL, encryption_algorithm=serialization.BestAvailableEncryption(ca_password)))
    with open(ca_file_cert, "wb") as f:
        f.write(certificate.public_bytes(serialization.Encoding.PEM))

    print("CA Bundle generation complete.")

def generate_intermediate_ca(country, state, locality, ca_file_private, ca_file_cert, int_private_key_file, int_public_key_file, ca_password):
    # Generate private key for intermediate CA
    private_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    print("Generating intermediate certificate...")
    subject = x509.Name([
        x509.NameAttribute(NameOID.COUNTRY_NAME, country),
        x509.NameAttribute(NameOID.STATE_OR_PROVINCE_NAME, state),
        x509.NameAttribute(NameOID.LOCALITY_NAME, locality),
        x509.NameAttribute(NameOID.ORGANIZATION_NAME, u"FEAR Application"),
        x509.NameAttribute(NameOID.COMMON_NAME, u"FEAR Intermediate CA"),
    ])

    with open(ca_file_private, "rb") as f:
        ca_private_key = serialization.load_pem_private_key(f.read(), password=ca_password)

    with open(ca_file_cert, "rb") as f:
        ca_certificate = x509.load_pem_x509_certificate(f.read())
        
    issuer = ca_certificate.subject
    certificate = x509.CertificateBuilder().subject_name(subject) \
        .issuer_name(issuer) \
        .public_key(private_key.public_key()) \
        .serial_number(x509.random_serial_number()) \
        .not_valid_before(datetime.utcnow()) \
        .not_valid_after(datetime.utcnow() + timedelta(days=3650)) \
        .add_extension(x509.BasicConstraints(ca=True, path_length=0), critical=True) \
        .sign(ca_private_key, hashes.SHA256())

    print(f"Saving intermediate CA private key to {int_private_key_file} and certificate to {int_public_key_file}...")
    with open(int_private_key_file, "wb") as f:
        f.write(private_key.private_bytes(encoding=serialization.Encoding.PEM, format=serialization.PrivateFormat.TraditionalOpenSSL, encryption_algorithm=serialization.BestAvailableEncryption(ca_password)))

    with open(int_public_key_file, "wb") as f:
        f.write(certificate.public_bytes(serialization.Encoding.PEM))

    print("Intermediate CA generation complete.")

def generate_ssl_certificate(signing_private_key, signing_certificate, certificate_password, output_folder, common_name, output_name):
    # Load signing private key and certificate
    with open(signing_private_key, "rb") as f:
        signing_key = serialization.load_pem_private_key(f.read(), password=certificate_password)

    with open(signing_certificate, "rb") as f:
        signing_cert = x509.load_pem_x509_certificate(f.read())

    # Generate private key for SSL certificate
    private_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)

    subject = x509.Name([
        x509.NameAttribute(NameOID.COMMON_NAME, common_name)
    ])

    issuer = signing_cert.subject

    certificate = x509.CertificateBuilder().subject_name(subject) \
        .issuer_name(issuer) \
        .public_key(private_key.public_key()) \
        .serial_number(x509.random_serial_number()) \
        .not_valid_before(datetime.utcnow()) \
        .not_valid_after(datetime.utcnow() + timedelta(days=365)) \
        .add_extension(x509.BasicConstraints(ca=False, path_length=None), critical=True) \
        .sign(signing_key, hashes.SHA256())

    private_key_file = os.path.join(output_folder, f"{output_name}.key")
    public_key_file = os.path.join(output_folder, f"{output_name}.crt")

    print(f"Saving SSL certificate private key to {private_key_file} and certificate to {public_key_file}...")
    
    with open(private_key_file, "wb") as f:
        f.write(private_key.private_bytes(encoding=serialization.Encoding.PEM, format=serialization.PrivateFormat.TraditionalOpenSSL, encryption_algorithm=serialization.NoEncryption()))

    with open(public_key_file, "wb") as f:
        f.write(certificate.public_bytes(serialization.Encoding.PEM))


def generate_rsa_private_key():
    private_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    # base64 encode the private key for storage or transmission
    pem_format = private_key.private_bytes(encoding=serialization.Encoding.PEM, format=serialization.PrivateFormat.TraditionalOpenSSL, encryption_algorithm=serialization.NoEncryption())

    #strip the begin and end private key lines
    pem_str = pem_format.decode('utf-8')
    pem_str = pem_str.replace("-----BEGIN RSA PRIVATE KEY-----", "").replace("-----END RSA PRIVATE KEY-----", "").replace("\n", "")
    return pem_str