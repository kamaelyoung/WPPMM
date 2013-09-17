#include "pch.h"
#include "Direct3DInterop.h"
#include "Direct3DContentProvider.h"

using namespace Windows::Foundation;
using namespace Windows::UI::Core;
using namespace Microsoft::WRL;
using namespace Windows::Phone::Graphics::Interop;
using namespace Windows::Phone::Input::Interop;

namespace WPPMMComp
{

Direct3DInterop::Direct3DInterop() :
	m_timer(ref new BasicTimer())
{
}

IDrawingSurfaceContentProvider^ Direct3DInterop::CreateContentProvider()
{
	ComPtr<Direct3DContentProvider> provider = Make<Direct3DContentProvider>(this);
	return reinterpret_cast<IDrawingSurfaceContentProvider^>(provider.Get());
}

// IDrawingSurfaceManipulationHandler
void Direct3DInterop::SetManipulationHost(DrawingSurfaceManipulationHost^ manipulationHost)
{
	manipulationHost->PointerPressed +=
		ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerPressed);

	manipulationHost->PointerMoved +=
		ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerMoved);

	manipulationHost->PointerReleased +=
		ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerReleased);
}

void Direct3DInterop::RenderResolution::set(Windows::Foundation::Size renderResolution)
{
	if (renderResolution.Width  != m_renderResolution.Width ||
		renderResolution.Height != m_renderResolution.Height)
	{
		m_renderResolution = renderResolution;

		if (m_renderer)
		{
			m_renderer->UpdateForRenderResolutionChange(m_renderResolution.Width, m_renderResolution.Height);
			RecreateSynchronizedTexture();
		}
	}
}

// �C�x���g �n���h���[
void Direct3DInterop::OnPointerPressed(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
{
	// �����ɃR�[�h��}�����܂��B
}

void Direct3DInterop::OnPointerMoved(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
{
	// �����ɃR�[�h��}�����܂��B
}

void Direct3DInterop::OnPointerReleased(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
{
	// �����ɃR�[�h��}�����܂��B
}

// Direct3DContentProvider �Ƃ̃C���^�[�t�F�C�X�ڑ�
HRESULT Direct3DInterop::Connect(_In_ IDrawingSurfaceRuntimeHostNative* host)
{
	m_renderer = ref new CubeRenderer();
	m_renderer->Initialize();
	m_renderer->UpdateForWindowSizeChange(WindowBounds.Width, WindowBounds.Height);
	m_renderer->UpdateForRenderResolutionChange(m_renderResolution.Width, m_renderResolution.Height);

	// �����_���[�̏�����������������Ƀ^�C�}�[���ċN�����܂��B
	m_timer->Reset();

	return S_OK;
}

void Direct3DInterop::Disconnect()
{
	m_renderer = nullptr;
}

HRESULT Direct3DInterop::PrepareResources(_In_ const LARGE_INTEGER* presentTargetTime, _Out_ BOOL* contentDirty)
{
	*contentDirty = true;

	return S_OK;
}

HRESULT Direct3DInterop::GetTexture(_In_ const DrawingSurfaceSizeF* size, _Inout_ IDrawingSurfaceSynchronizedTextureNative** synchronizedTexture, _Inout_ DrawingSurfaceRectF* textureSubRectangle)
{
	m_timer->Update();
	m_renderer->Update(m_timer->Total, m_timer->Delta);
	m_renderer->Render();

	RequestAdditionalFrame();

	return S_OK;
}

ID3D11Texture2D* Direct3DInterop::GetTexture()
{
	return m_renderer->GetTexture();
}

}
