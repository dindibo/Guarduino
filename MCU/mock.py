from flask import Flask, jsonify, request, make_response
from flask_jwt_extended import JWTManager, create_access_token, jwt_required, get_jwt_identity
import psycopg2
import bcrypt
import serial
import datetime
import json
import random

# Initialize Flask app
app = Flask(__name__)
app.config['JWT_SECRET_KEY'] = 'supersecretkey'  # Change this to a strong secret key
jwt = JWTManager(app)

# Database connection settings
DB_HOST = "localhost"
DB_NAME = "guarduino"
DB_USER = "postgres"
DB_PASSWORD = "your_password"

# Helper function to connect to the PostgreSQL database
def get_db_connection():
    conn = psycopg2.connect(
        host=DB_HOST,
        database=DB_NAME,
        user=DB_USER,
        password=DB_PASSWORD
    )
    return conn

# Check if sensor is open or closed (mock function)
isOpen = lambda: random.randint(0, 5) == 1

# Auth route for POST (authenticate user and return JWT)
@app.route('/auth', methods=['POST'])
def auth_user():
    data = request.get_json()
    username = data.get('username')
    password = data.get('password')

    # Connect to the database
    conn = get_db_connection()
    cursor = conn.cursor()

    # Fetch user from the database
    cursor.execute("SELECT id, password_hash FROM users WHERE username = %s", (username,))
    user = cursor.fetchone()

    # Close the database connection
    cursor.close()
    conn.close()

    if user is None:
        return jsonify({"msg": "Invalid username or password"}), 401

    user_id, password_hash = user

    # Check if the provided password matches the hashed password in the database
    if not bcrypt.checkpw(password.encode('utf-8'), password_hash.encode('utf-8')):
        return jsonify({"msg": "Invalid username or password"}), 401

    # Create JWT token if authentication is successful
    access_token = create_access_token(identity={'user_id': user_id})
    response = make_response(jsonify({"msg": "Login successful"}))
    response.set_cookie('jwt_token', access_token, httponly=True)
    return response

# Zones route for GET (mock response for testing)
@app.route('/zones', methods=['GET'])
def zones():
    response = dict()
    
    lst = [
        {
            'sensor_type': '0',
            'sensor_name': 'salon',
            'state': ('open' if not isOpen() else 'closed'),
        },
        {
            'sensor_type': '0',
            'sensor_name': 'room 1',
            'state': ('open' if not isOpen() else 'closed'),
        },
        {
            'sensor_type': '1',
            'sensor_name': 'mirpeset',
            'state': ('open' if not isOpen() else 'closed'),
        },
    ]
    
    print(lst)
    
    response['sensors'] = lst
    
    return jsonify(response)

# Run the Flask app
if __name__ == '__main__':
    app.run(debug=False, host='0.0.0.0')
