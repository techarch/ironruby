require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Enumerable#each_with_object" do
  ruby_version_is '1.9' do
    before :each do
      @values = [2, 5, 3, 6, 1, 4]
      @enum = EnumerableSpecs::Numerous.new(*@values)
      @initial = "memo"
    end 
  
    it "passes each element and its argument to the block" do
      acc = []
      @enum.each_with_object(@initial) do |elem, obj|
        obj.should equal(@initial)
        obj = 42
        acc << elem
      end.should equal(@initial)
      acc.should == @values
    end

    it "returns an enumerator if no block" do
      acc = []
      e = @enum.each_with_object(@initial)
      e.each do |elem, obj|
        obj.should equal(@initial)
        obj = 42
        acc << elem
      end.should equal(@initial)
      acc.should == @values
    end
  end  
end
