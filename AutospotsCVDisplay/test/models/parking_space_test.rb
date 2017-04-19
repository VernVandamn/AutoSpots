# == Schema Information
#
# Table name: parking_spaces
#
#  id             :integer          not null, primary key
#  parking_coords :text
#  image_url      :text
#  created_at     :datetime         not null
#  updated_at     :datetime         not null
#  latitude       :float
#  longitude      :float
#  name           :text
#

require 'test_helper'

class ParkingSpaceTest < ActiveSupport::TestCase
  # test "the truth" do
  #   assert true
  # end
end
