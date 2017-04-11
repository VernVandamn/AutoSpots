class ParkingSpace < ApplicationRecord

	# do this after create because the id is assigned once the object is created
	after_create :create_folders, on: :create

	after_destroy :remove_images, on: :destroy

	private
		def create_folders
			image_folder = "#{Rails.root}/app/assets/images/#{self.id}/"
			spots_folder = "#{Rails.root}/app/assets/images/#{self.id}/spots/"

		  FileUtils.mkdir_p(image_folder) unless File.directory?(image_folder)
		  FileUtils.mkdir_p(spots_folder) unless File.directory?(spots_folder)
		end

		def remove_images
			image_folder = "#{Rails.root}/app/assets/images/#{self.id}/"

			FileUtils.rm_rf(image_folder)
		end
end
