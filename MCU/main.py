from flask import Flask, jsonify, request, make_response
from flask_jwt_extended import JWTManager, create_access_token, jwt_required, get_jwt_identity
import psycopg2
import serial
import datetime
import json

CFG_PATH = '.\\cfg.json'
CFG_KEYS = ['listen_port', 'COM', 'baudrate']
cfg = None


def read_cfg():
    d = ''
    
    with open(CFG_PATH, 'r') as f:
        d = f.read()
        
    r = json.loads(d)
    
    for x in CFG_KEYS:
        if not x in r:
            raise RuntimeError('Configuration file missing key: ' + x)
        
    return r


# Parse configuration
cfg = read_cfg()

# Initialize Flask app
app = Flask(__name__)
app.config['JWT_SECRET_KEY'] = 'supersecretkey'  # Change this to a strong secret key
jwt = JWTManager(app)


# Serial COM setup (global variable)
print('OPEN')
print(cfg['COM'])
print(cfg['baudrate'])
ser = serial.Serial(cfg['COM'], baudrate=cfg['baudrate'], timeout=1)

# Postgres DB connection
def get_db_connection():
    conn = psycopg2.connect(
        host="localhost",
        database="guarduino_db",
        user="your_db_user",
        password="your_db_password"
    )
    return conn

# Static landing page
@app.route('/')
def landing_page():
    return '<h1>Guarduino v1</h1>'

# Auth route for POST (authenticate user and return JWT)
@app.route('/auth', methods=['POST'])
def auth_user():
    data = request.get_json()
    username = data.get('username')
    password = data.get('password')

    # Database lookup for user authentication
    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute('SELECT id, password FROM users WHERE username = %s', (username,))
    user = cur.fetchone()
    cur.close()
    conn.close()

    if user and user[1] == password:  # In production, password hashing should be used!
        user_id = user[0]
        access_token = create_access_token(identity={'user_id': user_id})
        response = make_response(jsonify({"msg": "Login successful"}))
        response.set_cookie('jwt_token', access_token, httponly=True)
        return response
    else:
        return jsonify({"msg": "Bad username or password"}), 401

# /status route to send command to Arduino and get the response
@app.route('/status', methods=['GET'])
@jwt_required()
def get_status():
    # Send the command "\x00\x00\x00\x01" to the Arduino
    command = b'\x00\x00\x00\x01'
    ser.write(command)
    arduino_response = ser.readline().decode('utf-8').strip()

    # Return Arduino's response to the user
    return jsonify({"status": arduino_response})

# Run the Flask app
if __name__ == '__main__':
    app.run(debug=True)
