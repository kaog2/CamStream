import cv2, json, math, base64, threading, os
import numpy as np
import subprocess as sp
import socketio as socketIOClient
#from flask_socketio import SocketIO, emit
from flask import Flask, render_template, Response, request, jsonify
from Clases.Camera import CameraStream
import cv2

app = Flask(__name__)
app._static_folder = os.path.abspath("templates/static/")

widthWpf = 1024
heightWpf = 768
widthWebApp = 640
heightWebApp = 480
clients = 0
isResolutionChanging = False

#FFMPEG
rtsp_url = 'rtsp://localhost:8554/mystream'
command = ['ffmpeg',
           '-re',
           '-y',
           '-f', 'rawvideo',
           '-vcodec', 'rawvideo',
           '-pix_fmt', 'bgr24',
           '-s', "{}x{}".format(widthWebApp, heightWebApp),
           #'-r', str(fps),
           '-i', '-',
           '-c:v', 'libx264',
           '-preset', 'ultrafast',
           '-tune', 'zerolatency',
           '-f', 'rtsp',
           #'-flags', 'local_headers', 
           '-rtsp_transport', 'tcp',
           #'-muxdelay', '0.1',
           '-bsf:v', 'dump_extra',
           rtsp_url]

def FfmpegEmit():
    p = sp.Popen(command, stdin=sp.PIPE)
    while cap:
        img = cap.read()
        cv2.line(img, lineHP1, lineHP2, (255,0,0), 2, lineType=8, shift=0)
        cv2.line(img, lineVP1, lineVP2, (0,0,255), 2, lineType=8, shift=0)
        p.stdin.write(img.tobytes())
        
    p.stdin.close()  # Close stdin pipe
    p.wait()  # Wait for FFmpeg sub-process to finish

#Init Line Web App
global lineHP1
global lineHP2
global lineVP1
global lineVP2

cap = CameraStream(0).start()

sio = socketIOClient.Client()
#sio.connect('http://127.0.0.1:3000',namespaces=['/video1'])
sio.connect('http://127.0.0.1:3000')

@sio.event
def connect():
    print("I'm connected!")

@app.route('/')
def index():
    """Video streaming home page."""
    return render_template('videoStream2.html')

@app.route('/stream1')
def stream1():
    """Video streaming home page."""
    return render_template('videoStream2.html')

def generateFrame():
    """Video streaming generator function."""
    while cap:
        frame = cap.read()
        convert = cv2.imencode('.jpg', frame)[1].tobytes()
        yield (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + convert + b'\r\n') # concate frame one by one and show result

@app.route('/wpfStream')
def video_feed():
    """Video streaming route. Put this in the src attribute of an img tag."""
    return Response(generateFrame(),
                    mimetype='multipart/x-mixed-replace; boundary=frame')

def EmitToSocket1():
    global lineHP1
    global lineHP2
    global lineVP1
    global lineVP2
    try:
        while cap:
            img = cap.read()
            cv2.line(img, lineHP1, lineHP2, (255,0,0), 2, lineType=8, shift=0)
            cv2.line(img, lineVP1, lineVP2, (0,0,255), 2, lineType=8, shift=0)
            image=cv2.imencode(".jpeg", img,[cv2.IMWRITE_JPEG_QUALITY, 60])[1]
            #image=cv2.imencode(".jpeg", img)[1]
            img64=base64.b64encode(image)
            imgenco64 = img64.decode("utf-8")
            sio.emit('stream', imgenco64)
            
            #stop emit
            #k = cv2.waitKey(10)
            #if k == 27:
                #sio.disconnect()
                #cv2.destroyAllWindows()
                #cap.release()
                #break

        print("end of while emit1")
    except Exception as e:
        print("Exception emit1")
        print("exeption: " + str(e))

resolutionList = [  {
                        'resolutionId': 1,
                        'resolutionName': '640 x 480',
                        'width': 640,
                        'height': 480
                    },
                    {
                        'resolutionId': 2,
                        'resolutionName': '640 x 360',
                        'width': 640,
                        'height': 360
                    },
                    {
                        'resolutionId': 3,
                        'resolutionName': '320 x 240',
                        'width': 320,
                        'height': 240
                    },
                    {
                        'resolutionId': 4,
                        'resolutionName': '1024 x 768',
                        'width': 1024,
                        'height': 768
                    },
                    {
                        'resolutionId': 5,
                        'resolutionName': '1280 x 720',
                        'width': 1280,
                        'height': 720
                    },
                    {
                        'resolutionId': 6,
                        'resolutionName': '1280 x 960',
                        'width': 1280,
                        'height': 960
                    },
                    {
                        'resolutionId': 7,
                        'resolutionName': '1920 x 1080',
                        'width': 1920,
                        'height': 1080
                    },
                    {
                        'resolutionId': 8,
                        'resolutionName': '2048 x 1536',
                        'width': 2048,
                        'height': 1536
                    },
                    {
                        'resolutionId': 9,
                        'resolutionName': '2592 x 1944',
                        'width': 2592,
                        'height': 1944
                    }
                ]

@app.route('/getResolutions', methods=['GET'])
def getResolutions():
    return jsonify(resolutionList)

@app.route('/changeResolution', methods=['POST'])
def changeResolution():
    global widthWpf
    global heightWpf
    global isResolutionChanging
    result = searchResolution(json.loads(json.dumps(resolutionList)),request.json['resolutionId'])
    if(result != None):
        widthWpf = result['width']
        heightWpf = result['height']
        isResolutionChanging = True
        return jsonify({'OK': 'resolution changed to ' + str(result['width']) + ' x ' + str(result['height'])})
    else:
        return jsonify({'Warning': 'resolution not found'})
    
def searchResolution(items,resolutionId):
 for item in items:
    if resolutionId == item['resolutionId']:
        return item

@app.route('/requests',methods=['POST','GET'])
def requests():
    global lineHP1
    global lineHP2
    global lineVP1
    global lineVP2
    #print(request.method)
    #global switch,camera
    if request.method == 'POST':
        if request.form.get('moveDown') == 'Move-Down':
            aux = lineHP1[1]+5
            lineHP1 = (lineHP1[0],aux)
            lineHP2 = (lineHP2[0],aux)
        elif request.form.get('moveUp') == 'Move-Up':
            aux = lineHP1[1]-5
            lineHP1 = (lineHP1[0],aux)
            lineHP2 = (lineHP2[0],aux)
        elif request.form.get('moveLeft') == 'Move-Left':
            aux = lineVP1[0]-5
            lineVP1 = (aux,lineVP1[1])
            lineVP2 = (aux,lineVP2[1])
        elif request.form.get('moveRigth') == 'Move-Rigth':
            aux = lineVP1[0]+5
            lineVP1 = (aux,lineVP1[1])
            lineVP2 = (aux,lineVP2[1])
                          
    return render_template('videoStream2.html')

if __name__ == '__main__':

    width  = cap.width
    height = cap.height

    lineHP1 = (0,math.ceil(height/2))
    lineHP2 = (width,math.ceil(height/2))
    lineVP1 = (math.ceil(width/2),0)
    lineVP2 = (math.ceil(width/2),height)

    socket1 = threading.Thread(target=EmitToSocket1)
    socket1.start()

    ffmpeg = threading.Thread(target=FfmpegEmit)
    ffmpeg.start()

    app.run(host='192.168.178.28',port=9999,threaded=True)