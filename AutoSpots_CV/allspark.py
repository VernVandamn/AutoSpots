import requests
import json
import cloudinary
import argparse
from cloudinary import uploader as up
import os, shutil
import sched, time
import requests
import base64

cloudinary.config(
	cloud_name= "ddzdneuxe",
	api_key= "555971697581325",
	api_secret= "6PjnLcArSUuziXJyz65DyDnWzls"
)

output_base = './output/'
jsonInput = { 'data': [] };
baseurl = "http://jamesljenk.pythonanywhere.com/"

#######################################################################
# Functions
#######################################################################
def getParkingInfo():
	# This will return a json object of all the parking spaces set up on the website
	parkingSpaces = requests.get('http://www.autospots.org/parking_spaces.json').json()

	jsonInput['count'] = len(parkingSpaces)

	# Do the following for every set up parking lot
	for space in parkingSpaces:
		pid = str(space['id'])
		name = str(space['name'])
		lat = str(space['latitude'])
		longi = str(space['longitude'])
		# print pid
		coords = json.loads(space['parking_coords'])
		imgUrl = str(space['image_url'])

		# First create the folders in the output folder that the images will go into
		picDir = output_base + pid + '/'
		spotsDir = picDir	+ 'spots/'
		if not os.path.exists(output_base):
			os.mkdir(output_base)
		if not os.path.exists(picDir):
			os.mkdir(picDir)
		if not os.path.exists(spotsDir):
			os.mkdir(spotsDir)

		# Download the referenced image into the new created folder
		imgLoc = picDir+'orig.jpg'
		with open(imgLoc, 'wb') as pic:
			# print 'imgUrl:', imgUrl
			r = requests.get(imgUrl, stream=True)

			if not r.ok:
				print r

			for block in r.iter_content(512):
				if not block:
					break
				pic.write(block)
			# pic.write(r.content)

		# Create the json file from the selecting parking spots we downloaded
		jsonLoc = picDir+'parkingSpots.json'
		with open(jsonLoc, 'wb') as jsonFile:
			json.dump(coords, jsonFile)

		# Then we need to add the info the the inner object
		spaceOutput = {
			'output':		picDir,
			'json': 		jsonLoc,
			'name': 		name,
			'imgLoc':		imgLoc,
			'id':				pid,
			'latitude':	lat,
			'longitude':longi
		}
		jsonInput['data'].append(spaceOutput)

	# save the object we created as a json file for the cv part to read
	with open('images.json', 'w') as fp:
		json.dump(jsonInput, fp)

# This function runs the Autospots computer vision on the images from the website
def runCV():
	os.system("python AutoSpots.py -i images.json")
	print 'Parking has been analyzed'

# This function clears the output folder of old images
def cleanup():
	shutil.rmtree('./output/')

# Pushes an update to a existing lot in the database for the app
def update_lot(image, lotID, spots):
    response = requests.post(baseurl+'update/image/'+str(lotID)+"/", json={'data': image})
    response = requests.get(baseurl+'update/'+str(lotID)+"/"+str(spots)+"/")

# Creates a lot for a lot that was just setup on the website
def newLot(name,image, spots, lat, long):
    response = requests.post(baseurl+'newlot/', json={'name': name, 'lat': lat, 'long': long, 'spots': spots, 'image': image})

# Upload results to first the app server and then cloudinary for the website
def uploadResults():
	print 'Uploading images to server'
	data = jsonInput['data']

	# Get the names and id's of the current list of lots to either update or create new
	response = requests.get(baseurl+'/lots/')
	plots = response.json()

	for space in data:
		# Send final image to the python server and lot string
		new = True
		lotID = ''
		for lot_tuple in plots:
			if lot_tuple[0] == space['name']:
				new = False
				lotID = lot_tuple[1]
				break;

		encoded_string = ""
		spots = ""
		with open(space['output']+'parkinglotbinaryarray.data', 'rb') as pldata:
			spots = pldata.readline()
		with open(space['output']+'final.png', "rb") as image_file:
		    encoded_string = base64.encodestring(image_file.read())

		if new:
			name = space['name']
			longitude = space['longitude']
			latitude = space['latitude']
			newLot(name, encoded_string.decode(), spots, latitude, longitude)
		else:
			update_lot(encoded_string.decode(), lotID, spots)

		# Upload images to Cloudinary
		# Upload final image
		respose = up.upload(
			space['output']+'final.png',
			tags=space['id']+'final',
			folder=space['id'],
			public_id='final'
		)
		# Upload crfrnn image
		respose = up.upload(
			space['output']+'output.png',
			folder=space['id'],
			public_id='output'
		)
		# Upload darknet image
		respose = up.upload(
			space['output']+'predictions.jpg',
			folder=space['id'],
			public_id='predictions'
		)
		spotsDir = space['output']+'spots/'
		for spot in os.listdir(output_base+space['id']+'/spots'):
			respose = up.upload(
				spotsDir+spot,
				tags=space['id']+'spot',
				folder=space['id']+'/spots',
				public_id=spot[:-4]
			)

# This is the function to repeat the whole analyze and update sequence every so often
def do_something(sc):
	# print "Doing stuff..."
	getParkingInfo()
	runCV()
	uploadResults()
	print jsonInput
	s.enter(300, 1, do_something)

def main():
	# Set up to force an update with a input argument
	# construct the argument parse and parse the arguments
	ap = argparse.ArgumentParser()
	ap.add_argument("-f", "--force", help="Force update to server", action="store_true")
	args = ap.parse_args()

	# load the image
	if args.force:
		print 'Forcing update to server'
		getParkingInfo()
		runCV()
		uploadResults()
	else:
		# Run Scheduler
		s = sched.scheduler(time.time, time.sleep)
		s.enter(300, 1, do_something)
		s.run()

if __name__ == "__main__":
  main()
