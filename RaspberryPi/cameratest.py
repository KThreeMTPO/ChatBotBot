import grpc
import picamera
import time
import videostream_pb2
import videostream_pb2_grpc

def send_video_stream():
    # Set up a gRPC channel to the external server running on your PC
    channel = grpc.insecure_channel('YOUR_PC_IP_ADDRESS:50051')
    stub = videostream_pb2_grpc.VideoStreamStub(channel)

    # Set up the PiCamera
    camera = picamera.PiCamera()
    camera.resolution = (640, 480)
    camera.framerate = 24

    # Stream the video frames
    for frame in camera.capture_continuous(raw_capture, format="bgr", use_video_port=True):
        # Convert the OpenCV frame to bytes
        frame_bytes = frame.tobytes()

        # Create and send the gRPC request
        request = videostream_pb2.VideoFrame(frame=frame_bytes)
        response = stub.StreamVideo(request)

        # Sleep for a short time to avoid overloading the server
        time.sleep(0.01)

if __name__ == '__main__':
    send_video_stream()


