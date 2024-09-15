from flask import Flask, jsonify, request, make_response
from flask_jwt_extended import JWTManager, create_access_token, jwt_required, get_jwt_identity
import psycopg2
import serial
import datetime
import json
import random



# Initialize Flask app
app = Flask(__name__)
app.config['JWT_SECRET_KEY'] = 'supersecretkey'  # Change this to a strong secret key
jwt = JWTManager(app)

isOpen = lambda : random.randint(0, 5) == 1

# Auth route for POST (authenticate user and return JWT)
@app.route('/auth', methods=['POST'])
def auth_user():
    user_id = 0
    access_token = create_access_token(identity={'user_id': user_id})
    response = make_response(jsonify({"msg": "Login successful"}))
    response.set_cookie('jwt_token', access_token, httponly=True)
    return response


# Auth route for POST (authenticate user and return JWT)
@app.route('/zones', methods=['GET'])
def zones():
    response = dict()
    
    lst = [
        
        {
            'sensor_type':'0',
            'sensor_name':'salon',
            'state':('open' if not isOpen() else 'closed'),
        } ,
        
        {
            'sensor_type':'0',
            'sensor_name':'room 1',
            'state':('open' if not isOpen() else 'closed'),
        } ,
        
        {
            'sensor_type':'1',
            'sensor_name':'mirpeset',
            'state':('open' if not isOpen() else 'closed'),
        } ,
        
    ]
    
    print(lst)
    
    response['sensors'] = lst
    
    return jsonify(response)


# Run the Flask app
if __name__ == '__main__':
    app.run(debug=False, host='0.0.0.0')
 