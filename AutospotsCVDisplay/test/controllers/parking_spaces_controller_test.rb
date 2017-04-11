require 'test_helper'

class ParkingSpacesControllerTest < ActionDispatch::IntegrationTest
  setup do
    @parking_space = parking_spaces(:one)
  end

  test "should get index" do
    get parking_spaces_url
    assert_response :success
  end

  test "should get new" do
    get new_parking_space_url
    assert_response :success
  end

  test "should create parking_space" do
    assert_difference('ParkingSpace.count') do
      post parking_spaces_url, params: { parking_space: { image_url: @parking_space.image_url, parking_coords: @parking_space.parking_coords } }
    end

    assert_redirected_to parking_space_url(ParkingSpace.last)
  end

  test "should show parking_space" do
    get parking_space_url(@parking_space)
    assert_response :success
  end

  test "should get edit" do
    get edit_parking_space_url(@parking_space)
    assert_response :success
  end

  test "should update parking_space" do
    patch parking_space_url(@parking_space), params: { parking_space: { image_url: @parking_space.image_url, parking_coords: @parking_space.parking_coords } }
    assert_redirected_to parking_space_url(@parking_space)
  end

  test "should destroy parking_space" do
    assert_difference('ParkingSpace.count', -1) do
      delete parking_space_url(@parking_space)
    end

    assert_redirected_to parking_spaces_url
  end
end
