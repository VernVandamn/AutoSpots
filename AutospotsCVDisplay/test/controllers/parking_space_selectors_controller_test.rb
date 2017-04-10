require 'test_helper'

class ParkingSpaceSelectorsControllerTest < ActionDispatch::IntegrationTest
  setup do
    @parking_space_selector = parking_space_selectors(:one)
  end

  test "should get index" do
    get parking_space_selectors_url
    assert_response :success
  end

  test "should get new" do
    get new_parking_space_selector_url
    assert_response :success
  end

  test "should create parking_space_selector" do
    assert_difference('ParkingSpaceSelector.count') do
      post parking_space_selectors_url, params: { parking_space_selector: { content: @parking_space_selector.content, user_id: @parking_space_selector.user_id } }
    end

    assert_redirected_to parking_space_selector_url(ParkingSpaceSelector.last)
  end

  test "should show parking_space_selector" do
    get parking_space_selector_url(@parking_space_selector)
    assert_response :success
  end

  test "should get edit" do
    get edit_parking_space_selector_url(@parking_space_selector)
    assert_response :success
  end

  test "should update parking_space_selector" do
    patch parking_space_selector_url(@parking_space_selector), params: { parking_space_selector: { content: @parking_space_selector.content, user_id: @parking_space_selector.user_id } }
    assert_redirected_to parking_space_selector_url(@parking_space_selector)
  end

  test "should destroy parking_space_selector" do
    assert_difference('ParkingSpaceSelector.count', -1) do
      delete parking_space_selector_url(@parking_space_selector)
    end

    assert_redirected_to parking_space_selectors_url
  end
end
