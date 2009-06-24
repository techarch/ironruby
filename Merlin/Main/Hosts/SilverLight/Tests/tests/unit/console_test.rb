describe 'Console regressions' do
  
  def inspect_object(obj)
    engine = IronRuby.get_engine(DynamicApplication.current.runtime)
    IronRuby.
      get_execution_context(engine).
      define_global_variable('__test_object__', obj)
    engine.
      create_script_source_from_string('$__test_object__.inspect.to_clr_string').
      execute
  end

  it 'should escape HTML property' do
    class Foo; end
    ReplOutputBuffer = Microsoft::Scripting::Silverlight::ReplOutputBuffer
    element = System::Windows::Browser::HtmlPage.document.create_element 'div'
    result = inspect_object(Foo.new)
    
    buffer = ReplOutputBuffer.new element, 'output'
    buffer.write(result)
    buffer.flush

    element.get_property('innerHTML').to_s.should.match(
      /<(DIV|div) class="?output"?>#&lt;Foo:0x[0-9a-f]+&gt;<\/(DIV|div)>/
    )
  end
end
