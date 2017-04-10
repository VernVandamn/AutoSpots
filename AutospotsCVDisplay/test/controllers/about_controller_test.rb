require 'test_helper'

class AboutControllerTest < ActionDispatch::IntegrationTest
  test "should get tutorial" do
    get about_tutorial_url
    assert_response :success
  end

  test "should get team" do
    get about_team_url
    assert_response :success
  end

end
