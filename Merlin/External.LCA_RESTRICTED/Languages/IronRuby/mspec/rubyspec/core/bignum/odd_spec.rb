require File.dirname(__FILE__) + '/../../spec_helper'

ruby_version_is "1.8.7" do
  describe "Bignum#odd?" do
    it "returns true if self is odd and positive" do
      (987279**19).odd?.should be_true
    end

    it "returns true if self is odd and negative" do
      (-9873389**97).odd?.should be_true
    end

    it "returns false if self is even and positive" do
      (10000000**10).odd?.should be_false
    end

    it "returns false if self is even and negative" do
      (-1000000**100).odd?.should be_false
    end
  end
end
